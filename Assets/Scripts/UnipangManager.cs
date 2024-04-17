using System.Collections.Generic;
using Unity.VisualScripting;
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

    Coords latestSwappedCoords;
    Direction latestSwappedDirection;

    /// ������ ���
    private GameObject[,] elementArray;
    //private GameObject[,] gridArray; // ���Ұ� �ƴ� �׸��� Ư������ ������ ������ ��´�

    void Start()
    {
        myCamera.transform.position = new Vector3((float)unipangGameCol / 2 - 0.5F, -(float)unipangGameRow / 2 + 0.5F, myCamera.transform.position.z);
        myCamera.orthographicSize = unipangGameCol;
        elementArray = new GameObject[unipangGameRow, unipangGameCol];

        latestSwappedCoords.row = -1;
        latestSwappedCoords.col = -1;

        // �ʱ�ȭ ���� ����
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1); c++)
            {
                // ���� ������Ʈ ����
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

    /// [Visual Effects] ����� ��� ��ġ ������ ��ĳ� ���ҵ鿡�� �˷��ְ� �ش� ��ġ�� �̵��ϰ� �մϴ�
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

    /// 01 �迭�� ��� 3���̻� �ݺ����ҵ��� ���� + ������ ����
    void AddCoordsToHashSet_Row(HashSet<Coords> mySet, int currentRow, int baseIndex, int succession) // succession = 2�� 3����, succession = 3�� 4����, ...
    {
        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;
            newCoord.row = currentRow;
            newCoord.col = i;

            if (mySet.Contains(newCoord))
            {
                // ��ź ������ ����
                elementArray[currentRow, i].GetComponent<ElementBase>().AttachItem(AttachableItem.BOMB);
                mySet.Remove(newCoord); // ó�� ��ź�� �� ���Ҵ� �� ������� ���� ����
            }
            else
            {
                mySet.Add(newCoord);
            }
        }

        if (succession == 3)
        {
            int randomInt = Random.Range(0, 4);
            elementArray[currentRow, baseIndex + randomInt].GetComponent<ElementBase>().AttachItem(AttachableItem.ROWCLEAR);
        }
    }
    void AddCoordsToHashSet_Column(HashSet<Coords> mySet, int currentColumn, int baseIndex, int succession)
    {
        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;
            newCoord.row = i;
            newCoord.col = currentColumn;

            if (mySet.Contains(newCoord))
            {
                // ��ź ������ ����
                elementArray[i, currentColumn].GetComponent<ElementBase>().AttachItem(AttachableItem.BOMB);
                mySet.Remove(newCoord); // ó�� ��ź�� �� ���Ҵ� �� ������� ���� ����
            }
            else
            {
                mySet.Add(newCoord);
            }
        }

        if (succession == 3)
        {
            int randomInt = Random.Range(0, 4);
            elementArray[baseIndex + randomInt, currentColumn].GetComponent<ElementBase>().AttachItem(AttachableItem.ROWCLEAR);
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
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Row(mySet, rowIndex, baseIndex, succession);
                baseIndex = c;
                currentBase = elementArray[rowIndex, c].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Row(mySet, rowIndex, baseIndex, succession);
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
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Column(mySet, columnIndex, baseIndex, succession);
                baseIndex = r;
                currentBase = elementArray[r, columnIndex].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Column(mySet, columnIndex, baseIndex, succession);
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
                SwapElementsInArray(r, c, Direction.RIGHT);

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElementsInArray(r, c, Direction.RIGHT);
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElementsInArray(r, c, Direction.RIGHT);
            }
        }

        // �Ҽ��ִ� ���ν����� ���غ���
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            for (int r = 0; r < elementArray.GetLength(0) - 1; r++)
            {
                SwapElementsInArray(r, c, Direction.DOWN);

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElementsInArray(r, c, Direction.DOWN);
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElementsInArray(r, c, Direction.DOWN);
            }
        }

        return false;
    }

    /// [���� ���� ����]
    bool SwapElementsInArray(int row, int col, Direction dir)
    {
        // ����ó����
        if (row < 0 || row >= elementArray.GetLength(0)) return false;
        if (col < 0 || col >= elementArray.GetLength(1)) return false;

        int swapIndexRow = row;
        int swapIndexCol = col;

        switch (dir)
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
        GameObject temp = elementArray[row, col];
        elementArray[row, col] = elementArray[swapIndexRow, swapIndexCol];
        elementArray[swapIndexRow, swapIndexCol] = temp;

        return true;
    }

    // �׽�Ʈ
    public GameObject latestSwappedCoordsVisualizer1;
    public GameObject latestSwappedCoordsVisualizer2;
    void MoveVisualizers()
    {
        latestSwappedCoordsVisualizer1.transform.position = new Vector3(latestSwappedCoords.col, -latestSwappedCoords.row, latestSwappedCoordsVisualizer1.transform.position.z);

        switch (latestSwappedDirection)
        {
            case Direction.UP:
                latestSwappedCoordsVisualizer2.transform.position = new Vector3(latestSwappedCoords.col, -(latestSwappedCoords.row - 1), latestSwappedCoordsVisualizer1.transform.position.z);
                break;
            case Direction.DOWN:
                latestSwappedCoordsVisualizer2.transform.position = new Vector3(latestSwappedCoords.col, -(latestSwappedCoords.row + 1), latestSwappedCoordsVisualizer1.transform.position.z);
                break;
            case Direction.LEFT:
                latestSwappedCoordsVisualizer2.transform.position = new Vector3(latestSwappedCoords.col - 1, -latestSwappedCoords.row, latestSwappedCoordsVisualizer1.transform.position.z);
                break;
            case Direction.RIGHT:
                latestSwappedCoordsVisualizer2.transform.position = new Vector3(latestSwappedCoords.col + 1, -latestSwappedCoords.row, latestSwappedCoordsVisualizer1.transform.position.z);
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Search
        {
            // ���� �� �˻��ϰ� ǥ�� + ������ ����
            HashSet<Coords> coordsToPop = GetSuccessiveCoordsInArr();

            for (int r = 0; r < elementArray.GetLength(0); r++)
            {
                foreach (Coords element in coordsToPop)
                {
                    if (element.row == r)
                    {
                        elementArray[r, element.col].GetComponent<ElementBase>().ShowAsTarget();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Clear
        {
            // ǥ���� �� ����� + �Ͷ߷��� ������ ����ϱ�
            ClearAndPushNew(GetSuccessiveCoordsInArr());
            UpdateElementCoordsInfo();
        }

        // �巡�׷� ���� ��ǲ �ޱ�
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 latestMouseDownPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
            latestClickedCoords.row = -(int)Mathf.Round(latestMouseDownPosition.y);
            latestClickedCoords.col = (int)Mathf.Round(latestMouseDownPosition.x);
            Debug.Log($"latestSelectedCoords: [{latestClickedCoords.row}, {latestClickedCoords.col}]");
        }

        if (0 <= latestClickedCoords.row && latestClickedCoords.row < elementArray.GetLength(0)
            && 0 <= latestClickedCoords.col && latestClickedCoords.col < elementArray.GetLength(1))
        {
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mouseUpPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
                int rowToSwap = -(int)Mathf.Round(mouseUpPosition.y);
                int colToSwap = (int)Mathf.Round(mouseUpPosition.x);

                if (latestClickedCoords.col == colToSwap)
                {
                    if (latestClickedCoords.row > rowToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords.row, latestClickedCoords.col, Direction.UP);
                        latestSwappedDirection = Direction.UP;
                    }
                    else if (latestClickedCoords.row < rowToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords.row, latestClickedCoords.col, Direction.DOWN);
                        latestSwappedDirection = Direction.DOWN;
                    }
                    latestSwappedCoords = latestClickedCoords;
                    MoveVisualizers();
                }
                else if (latestClickedCoords.row == rowToSwap)
                {
                    if (latestClickedCoords.col > colToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords.row, latestClickedCoords.col, Direction.LEFT);
                        latestSwappedDirection = Direction.LEFT;
                    }
                    else if (latestClickedCoords.col < colToSwap)
                    {
                        SwapElementsInArray(latestClickedCoords.row, latestClickedCoords.col, Direction.RIGHT);
                        latestSwappedDirection = Direction.RIGHT;
                    }
                    latestSwappedCoords = latestClickedCoords;
                    MoveVisualizers();
                }
            }
            UpdateElementCoordsInfo();
        }
    }
}
