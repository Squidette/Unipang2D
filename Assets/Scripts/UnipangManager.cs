using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

struct Coords
{
    public int row;
    public int col;

    public Coords(int row, int column)
    {
        this.row = row;
        col = column;
    }
}

public class UnipangManager : MonoBehaviour
{
    public Camera myCamera;
    public GameObject elementPrefab;

    /// ���� �ʱ�ȭ ����

    // ������ ���� ����
    public int numOfElementTypes;

    // ������ ��� ���� ����
    public int unipangGameRow;
    public int unipangGameCol;

    // �Է� ����
    Coords latestClickedCoords;

    Coords latestSwappedCoords1;
    Coords latestSwappedCoords2;

    //public bool canSwap = true;
    //bool firstSwapActions = false; // true�� �Ǿ����� ù ��° ���ҽÿ��� ������ ȿ������ �ߵ���ų ���̴�

    /// ������ ���
    private GameObject[,] elementArray;
    //private GameObject[,] gridArray; // ���Ұ� �ƴ� �׸��� Ư������ ������ ������ ��´�

    void Start()
    {
        myCamera.transform.position = new Vector3((float)unipangGameCol / 2 - 0.5F, -(float)unipangGameRow / 2 + 0.5F, myCamera.transform.position.z);
        myCamera.orthographicSize = unipangGameCol;
        elementArray = new GameObject[unipangGameRow, unipangGameCol];

        latestSwappedCoords1.row = -1;
        latestSwappedCoords1.col = -1;

        // �ʱ�ȭ ���� ����
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1); c++)
            {
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // Ÿ�� �����ϱ�
                element.GetComponent<ElementBase>().SetType(UnityEngine.Random.Range(0, numOfElementTypes)); // ���� +1�� �Ѹ���, +2�� �������̴�

                elementArray[r, c] = element;
            }
        }

        while (GetSuccessiveCoordsInArr().Count != 0)
        {
            ClearAndPushNew(GetSuccessiveCoordsInArr());
        }

        UpdateElementCoordsInfo();
    }

    /// 01 ����� ��� ��ġ ������ ��ĳ� ���ҵ鿡�� �˷��ְ� �ش� ��ġ�� �̵��ϰ� ��
    void UpdateElementCoordsInfo()
    {
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1); c++)
            {
                elementArray[r, c].GetComponent<ElementBase>().MoveTo(r, c);
            }
        }
    }

    /// [������ �ٽ� ����]

    /// 02 ������ �������� ȿ�� �ߵ�

    // Coords�� �׸��� ���� ��ȿ�� ��ǥ���� Ȯ���ϰ� �ؽ��¿� �߰�
    void AddCoordsToHashSet_s(HashSet<Coords> mySet, Coords coord)
    {
        if (coord.row < 0) return;
        if (elementArray.GetLength(0) <= coord.row) return;
        if (coord.col < 0) return;
        if (elementArray.GetLength(1) <= coord.col) return;

        if (elementArray[coord.row, coord.col].GetComponent<ElementBase>().type >= numOfElementTypes) return;

        mySet.Add(coord);
    }

    // �������� ������ ȿ���� �ߵ����ش� (�� �Ͷ߸���, �� �Ͷ߸���, ��ź �Ͷ߸���) - �ؽ��¿� ��ȭ�� ������ false�� ��ȯ�Ѵ�
    bool AddItemEffectsToHashSet(HashSet<Coords> mySet)
    {
        HashSet<Coords> setToBePoppedByItems = new HashSet<Coords>();

        foreach (Coords element in mySet)
        {
            if (elementArray[element.row, element.col].GetComponent<ElementBase>().attachedItemType == AttachableItem.ROWCLEAR)
            {
                for (int c = 0; c < elementArray.GetLength(1); c++)
                {
                    AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row, c));
                }
            }
            else if (elementArray[element.row, element.col].GetComponent<ElementBase>().attachedItemType == AttachableItem.COLUMNCLEAR)
            {
                for (int r = 0; r < elementArray.GetLength(0); r++)
                {
                    AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(r, element.col));
                }
            }
            else if (elementArray[element.row, element.col].GetComponent<ElementBase>().attachedItemType == AttachableItem.BOMB)
            {
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row - 1, element.col - 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row - 1, element.col + 0));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row - 1, element.col + 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row, element.col - 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row, element.col + 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row + 1, element.col - 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row + 1, element.col + 0));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row + 1, element.col + 1));
            }
        }

        if (setToBePoppedByItems == null) return false;

        foreach (Coords element in setToBePoppedByItems)
        {
            mySet.Add(element);
        }

        return true;
    }
    
    /// 03 �迭�� ��� 3���̻� �ݺ����ҵ��� ���� + ������ ����
    void AddCoordsToHashSetAndAttachItem_Row(HashSet<Coords> mySet, int currentRow, int baseIndex, int succession) // succession = 2�� 3����, succession = 3�� 4����, ...
    {
        int rowClearItemIndex = -1;
        if (succession == 3)
        {
            rowClearItemIndex = baseIndex + Random.Range(0, 4);
        }

        int jellyBeanItemIndex = -1;
        if (succession >= 4)
        {
            jellyBeanItemIndex = baseIndex + 2;
        }

        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;
            newCoord.row = currentRow;
            newCoord.col = i;

            // ������ ������ ����
            if (jellyBeanItemIndex != -1 && i == jellyBeanItemIndex)
            {
                elementArray[currentRow, i].GetComponent<ElementBase>().SetType(numOfElementTypes + 1);
                mySet.Remove(newCoord);
            }
            // �� Ŭ���� ������ ����
            else if (rowClearItemIndex != -1 && i == rowClearItemIndex)
            {
                elementArray[currentRow, i].GetComponent<ElementBase>().AttachItem(AttachableItem.ROWCLEAR);
                mySet.Remove(newCoord);
            }
            // ��ź ������ ����
            else if (mySet.Contains(newCoord))
            {
                elementArray[currentRow, i].GetComponent<ElementBase>().AttachItem(AttachableItem.BOMB);
                mySet.Remove(newCoord);
            }
            else
            {
                mySet.Add(newCoord);
            }
        }
    }

    void AddCoordsToHashSetAndAttachItem_Column(HashSet<Coords> mySet, int currentColumn, int baseIndex, int succession) // succession = 2�� 3����, succession = 3�� 4����, ...
    {
        int columnClearItemIndex = -1;
        if (succession == 3)
        {
            columnClearItemIndex = baseIndex + Random.Range(0, 4);
        }

        int jellyBeanItemIndex = -1;
        if (succession >= 4)
        {
            jellyBeanItemIndex = baseIndex + 2;
        }

        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;
            newCoord.row = i;
            newCoord.col = currentColumn;

            // ������ ������ ����
            if (jellyBeanItemIndex != -1 && i == jellyBeanItemIndex)
            {
                elementArray[i, currentColumn].GetComponent<ElementBase>().SetType(numOfElementTypes + 1);
                mySet.Remove(newCoord);
            }
            // �� Ŭ���� ������ ����
            else if (columnClearItemIndex != -1 && i == columnClearItemIndex)
            {
                elementArray[i, currentColumn].GetComponent<ElementBase>().AttachItem(AttachableItem.COLUMNCLEAR);
                mySet.Remove(newCoord);
            }
            // ��ź ������ ����
            else if (mySet.Contains(newCoord))
            {
                elementArray[i, currentColumn].GetComponent<ElementBase>().AttachItem(AttachableItem.BOMB);
                mySet.Remove(newCoord);
            }
            else
            {
                mySet.Add(newCoord);
            }
        }
    }

    void AddRowSuccessionToHashSet(HashSet<Coords> mySet, int rowIndex)
    {
        int baseIndex = 0;
        int currentBase = elementArray[rowIndex, 0].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int c = 1; c < elementArray.GetLength(1); c++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (elementArray[rowIndex, c].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� �ش� ������ �ؽü¿� �߰�
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Row(mySet, rowIndex, baseIndex, succession);
                baseIndex = c;
                currentBase = elementArray[rowIndex, c].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Row(mySet, rowIndex, baseIndex, succession);
    }

    void AddColumnSuccessionToHashSet(HashSet<Coords> mySet, int columnIndex)
    {
        int baseIndex = 0;
        int currentBase = elementArray[0, columnIndex].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int r = 1; r < elementArray.GetLength(0); r++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (elementArray[r, columnIndex].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� �ش� ������ �ؽü¿� �߰�
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Column(mySet, columnIndex, baseIndex, succession);
                baseIndex = r;
                currentBase = elementArray[r, columnIndex].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Column(mySet, columnIndex, baseIndex, succession);
    }

    HashSet<Coords> GetSuccessiveCoordsInArr()
    {
        HashSet<Coords> successiveCoords = new HashSet<Coords>();

        // 1. �ະ �˻�
        for (int r = 0; r < elementArray.GetLength(0); r++)
            AddRowSuccessionToHashSet(successiveCoords, r);

        // 2. ���� �˻�
        for (int c = 0; c < elementArray.GetLength(1); c++)
            AddColumnSuccessionToHashSet(successiveCoords, c);

        return successiveCoords;
    }

    /// 02 �Է¹��� �ڸ��� ���ҵ��� ����� ���� ���ҵ��� �Ʒ��� �������� ��, ���� ���ڸ��� �� ���� ���ҷ� ä��
    void ClearAndPushNew(HashSet<Coords> coordsToPop)
    {
        if (coordsToPop.Count == 0) return;

        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            // �Էµ� �ؽü¿��� ���� ��ġ�ϴ� ���ҵ��� ã�� �������� �� ������ ����
            List<int> rowToErase = new List<int>();
            foreach (Coords element in coordsToPop)
            {
                if (element.col == c) rowToErase.Add(element.row);
            }
            if (rowToErase.Count == 0) continue;

            // �������� �ʰ� �����ִ� ���ҵ��� �� �Ʒ������� �о����
            // int emptySpace�� �о������ ĭ���� ����ϸ鼭 ���� ���� �ε������� ���� �ε������� �湮�ϸ� �Ʒ����� �׾ư��� ������ ��ġ�� �����
            int emptySpace = 0;
            for (int r = elementArray.GetLength(0) - 1; r >= 0; r--)
            {
                if (rowToErase.Contains(r))
                {
                    emptySpace++;
                }
                else
                {
                    if (emptySpace != 0)
                    {
                        GameObject temp = elementArray[r, c];
                        elementArray[r, c] = elementArray[r + emptySpace, c];
                        elementArray[r + emptySpace, c] = temp;
                    }
                }
            }

            // ���� ���������� rowToErase.Count���� �ڸ��� ���� ���ҵ��̹Ƿ� ���ο� ���� ���ҵ�� ä�� ��
            for (int r = 0; r < rowToErase.Count; r++)
            {
                // ���� ������Ʈ ����
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // Ÿ�� �����ϱ�
                element.GetComponent<ElementBase>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                // �迭�� �߰��ϱ�
                Destroy(elementArray[r, c]);
                elementArray[r, c] = element;

                // �ش� ��ġ�� �ű��
                element.GetComponent<ElementBase>().MoveTo(r, c);
            }
        }
    }

    /// 03 �ѹ��� �������� ������ ����� �ֳ� �Ǻ��ϴ� �Լ� (���� �׽�Ʈ ���غ�)
    bool SuccessionExistsInRow(int rowIndex)
    {
        int baseIndex = 0;
        int currentBase = elementArray[rowIndex, 0].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int c = 1; c < elementArray.GetLength(1); c++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (elementArray[rowIndex, c].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� return true
                if (succession >= 2) return true;
                baseIndex = c;
                currentBase = elementArray[rowIndex, c].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2) return true;

        return false;
    }
    bool SuccessionExistsInColumn(int columnIndex)
    {
        int baseIndex = 0;
        int currentBase = elementArray[0, columnIndex].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int r = 1; r < elementArray.GetLength(0); r++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (elementArray[r, columnIndex].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� return true
                if (succession >= 2) return true;
                baseIndex = r;
                currentBase = elementArray[r, columnIndex].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2) return true;

        return false;
    }
    bool SwappingMakesSuccession()
    {
        // �Ҽ��ִ� ���ν����� ���غ���
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1) - 1; c++)
            {
                SwapElementsInArray(new Coords(r, c), Direction.RIGHT);

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElementsInArray(new Coords(r, c), Direction.RIGHT);
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElementsInArray(new Coords(r, c), Direction.RIGHT);
            }
        }

        // �Ҽ��ִ� ���ν����� ���غ���
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            for (int r = 0; r < elementArray.GetLength(0) - 1; r++)
            {
                SwapElementsInArray(new Coords(r, c), Direction.DOWN);

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElementsInArray(new Coords(r, c), Direction.DOWN);
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElementsInArray(new Coords(r, c), Direction.DOWN);
            }
        }

        return false;
    }

    /// [���� ���� ����]
    bool SwapElementsInArray(Coords selectedCoords, Direction directionToChange)
    {
        // ����ó����
        if (selectedCoords.row < 0 || selectedCoords.row >= elementArray.GetLength(0)) return false;
        if (selectedCoords.col < 0 || selectedCoords.col >= elementArray.GetLength(1)) return false;

        int swapIndexRow = selectedCoords.row;
        int swapIndexCol = selectedCoords.col;

        switch (directionToChange)
        {
            case Direction.UP:
                swapIndexRow--;
                break;
            case Direction.DOWN:
                swapIndexRow++;
                break;
            case Direction.LEFT:
                swapIndexCol--;
                break;
            case Direction.RIGHT:
                swapIndexCol++;
                break;
        }

        if (swapIndexRow < 0 || swapIndexRow >= elementArray.GetLength(0)) return false;
        if (swapIndexCol < 0 || swapIndexCol >= elementArray.GetLength(1)) return false;

        // ����ó������ �հ� ������� �Դٸ� �ٲ���
        GameObject temp = elementArray[selectedCoords.row, selectedCoords.col];
        elementArray[selectedCoords.row, selectedCoords.col] = elementArray[swapIndexRow, swapIndexCol];
        elementArray[swapIndexRow, swapIndexCol] = temp;

        return true;
    }

    // �׽�Ʈ
    public GameObject latestSwappedCoordsVisualizer1;
    public GameObject latestSwappedCoordsVisualizer2;
    void MoveVisualizers()
    {
        latestSwappedCoordsVisualizer1.transform.position = new Vector3(latestSwappedCoords1.col, -latestSwappedCoords1.row, latestSwappedCoordsVisualizer1.transform.position.z);
        latestSwappedCoordsVisualizer2.transform.position = new Vector3(latestSwappedCoords2.col, -latestSwappedCoords2.row, latestSwappedCoordsVisualizer2.transform.position.z);
    }

    HashSet<Coords> cp;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) // Search
        {
            // ���� �� �˻��ϰ� ǥ�� + ������ ����
            cp = GetSuccessiveCoordsInArr();

            foreach (Coords element in cp)
            {
                elementArray[element.row, element.col].GetComponent<ElementBase>().ShowAsTarget();
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            AddItemEffectsToHashSet(cp);

            foreach (Coords element in cp)
            {
                elementArray[element.row, element.col].GetComponent<ElementBase>().ShowAsTarget();
            }
        }

        if (Input.GetKeyDown(KeyCode.M)) // Clear
        {
            // ǥ���� �� ����� + �Ͷ߷��� ������ ����ϱ�
            ClearAndPushNew(cp);
            UpdateElementCoordsInfo();
        }

        // �巡�׷� ���� ��ǲ �ޱ�
        if (Input.GetMouseButtonDown(0)) // ���콺 �ٿ�
        {
            Vector3 latestMouseDownPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
            latestClickedCoords.row = -(int)Mathf.Round(latestMouseDownPosition.y);
            latestClickedCoords.col = (int)Mathf.Round(latestMouseDownPosition.x);
        }

        if (0 <= latestClickedCoords.row && latestClickedCoords.row < elementArray.GetLength(0) // ��ȿ�� ĭ�� �����߰�
            && 0 <= latestClickedCoords.col && latestClickedCoords.col < elementArray.GetLength(1))
        {
            if (Input.GetMouseButtonUp(0)) // ���콺 ��
            {
                Vector3 mouseUpPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
                int rowToSwap = -(int)Mathf.Round(mouseUpPosition.y);
                int colToSwap = (int)Mathf.Round(mouseUpPosition.x);

                if (latestClickedCoords.col == colToSwap)
                {
                    if (latestClickedCoords.row > rowToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords, Direction.UP);

                        latestSwappedCoords1 = latestClickedCoords;
                        latestSwappedCoords2 = latestSwappedCoords1;
                        latestSwappedCoords2.row--;
                        MoveVisualizers();
                    }
                    else if (latestClickedCoords.row < rowToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords, Direction.DOWN);

                        latestSwappedCoords1 = latestClickedCoords;
                        latestSwappedCoords2 = latestSwappedCoords1;
                        latestSwappedCoords2.row++;
                        MoveVisualizers();
                    }
                }
                else if (latestClickedCoords.row == rowToSwap)
                {
                    if (latestClickedCoords.col > colToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords, Direction.LEFT);

                        latestSwappedCoords1 = latestClickedCoords;
                        latestSwappedCoords2 = latestSwappedCoords1;
                        latestSwappedCoords2.col--;
                        MoveVisualizers();
                    }
                    else if (latestClickedCoords.col < colToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords, Direction.RIGHT);

                        latestSwappedCoords1 = latestClickedCoords;
                        latestSwappedCoords2 = latestSwappedCoords1;
                        latestSwappedCoords2.col++;
                        MoveVisualizers();
                    }
                }
            }
            UpdateElementCoordsInfo();
        }
        
    }
}