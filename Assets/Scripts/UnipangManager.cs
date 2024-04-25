using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//public enum Direction
//{
//    UP,
//    DOWN,
//    LEFT,
//    RIGHT
//}

public struct Coords
{
    public int row;
    public int col;

    public Coords(int row, int column)
    {
        this.row = row;
        col = column;
    }

    public Coords(Coords c)
    {
        this.row = c.row;
        this.col = c.col;
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

    /// ���� ���� ����
    Coords selectedCoords;
    Coords swapCoords;

    public bool canSwap;
    bool firstSwapActionsDone; // ù ��° ���ҽÿ��� ������ ȿ������ �ߵ���Ű�� ���� �Һ���

    /// ������ ���
    private GameObject[,] elementArray;
    //private GameObject[,] gridArray; // ���Ұ� �ƴ� �׸��� Ư������ ������ ������ ��´�

    void Start()
    {
        canSwap = true;
        firstSwapActionsDone = true;

        myCamera.transform.position = new Vector3((float)unipangGameCol / 2 - 0.5F, -(float)unipangGameRow / 2 + 0.5F, myCamera.transform.position.z);
        myCamera.orthographicSize = unipangGameCol;
        elementArray = new GameObject[unipangGameRow, unipangGameCol];

        // �ʱ�ȭ ���� ����
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1); c++)
            {
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);
                element.transform.SetParent(transform);

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
                elementArray[r, c].GetComponent<ElementBase>().MoveTo_Instant(new Coords(r, c));
            }
        }
    }

    /// [������ �ٽ� ����]

    /// 01 �迭�� ��� 3���̻� �ݺ����ҵ��� ���� + ������ ���� - ������ �ѹ� �����Ѵ�
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

    HashSet<Coords> GetSuccessiveCoordsInArr() // <- �ٽ� �Լ�!
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

    /// 02 ������ �������� ȿ�� �ߵ� - ���̻� �߰��Ǵ� �������� ���� ������ ���������� ����
    public bool AreCoordsValidArrayMember(Coords coords)
    {
        if (coords.row < 0) return false;
        if (elementArray.GetLength(0) <= coords.row) return false;
        if (coords.col < 0) return false;
        if (elementArray.GetLength(1) <= coords.col) return false;

        return true;
    }

    // ���޹��� ��ǥ�� �׸��� ���� ��ȿ�� ��ǥ���� Ȯ���ϰ� �ش� ��ǥ�� ���Ҹ� �����ϰ� �ؽ��¿� �߰�
    void AddCoordsToHashSet_s(HashSet<Coords> mySet, Coords coords)
    {
        if (!AreCoordsValidArrayMember(coords)) return;
        if (elementArray[coords.row, coords.col].GetComponent<ElementBase>().type >= numOfElementTypes) return;

        mySet.Add(coords);
    }

    // �������� ������ ȿ���� �ߵ����ش� (�� �Ͷ߸���, �� �Ͷ߸���, ��ź �Ͷ߸���) <- �ٽ� �Լ�!
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
                elementArray[element.row, element.col].GetComponent<ElementBase>().AttachItem(AttachableItem.NONE);
            }
            else if (elementArray[element.row, element.col].GetComponent<ElementBase>().attachedItemType == AttachableItem.COLUMNCLEAR)
            {
                for (int r = 0; r < elementArray.GetLength(0); r++)
                {
                    AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(r, element.col));
                }
                elementArray[element.row, element.col].GetComponent<ElementBase>().AttachItem(AttachableItem.NONE);
            }
            else if (elementArray[element.row, element.col].GetComponent<ElementBase>().attachedItemType == AttachableItem.BOMB)
            {
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row - 1, element.col - 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row - 1, element.col));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row - 1, element.col + 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row, element.col - 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row, element.col + 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row + 1, element.col - 1));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row + 1, element.col));
                AddCoordsToHashSet_s(setToBePoppedByItems, new Coords(element.row + 1, element.col + 1));

                elementArray[element.row, element.col].GetComponent<ElementBase>().AttachItem(AttachableItem.NONE);
            }
        }

        if (setToBePoppedByItems.Count == 0) return false;

        foreach (Coords element in setToBePoppedByItems)
        {
            mySet.Add(element);
        }
        return true;
    }

    /// 03 �Է¹��� �ڸ��� ���ҵ��� ����� ���� ���ҵ��� �Ʒ��� �������� ��, ���� ���ڸ��� �� ���� ���ҷ� ä�� - ������ ȿ���� ���� ��ģ �� �� �� �����Ѵ�
    void ClearAndPushNew(HashSet<Coords> coordsToPop) // <- �ٽ� �Լ�!
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
                element.transform.SetParent(transform);

                // Ÿ�� �����ϱ�
                element.GetComponent<ElementBase>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                // �迭�� �߰��ϱ�
                Destroy(elementArray[r, c]);
                elementArray[r, c] = element;

                // �ش� ��ġ�� �ű��
                element.GetComponent<ElementBase>().MoveTo_Instant(new Coords(r, c));
            }
        }
    }

    /// 04 �ѹ��� �������� ������ ����� �ֳ� �Ǻ��ϴ� �Լ� (���� �׽�Ʈ ���غ�)
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
                SwapElementsInArray(new Coords(r, c), new Coords(r, c + 1));

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElementsInArray(new Coords(r, c), new Coords(r, c + 1));
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElementsInArray(new Coords(r, c), new Coords(r, c + 1));
            }
        }

        // �Ҽ��ִ� ���ν����� ���غ���
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            for (int r = 0; r < elementArray.GetLength(0) - 1; r++)
            {
                SwapElementsInArray(new Coords(r, c), new Coords(r + 1, c));

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElementsInArray(new Coords(r, c), new Coords(r + 1, c));
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElementsInArray(new Coords(r, c), new Coords(r + 1, c));
            }
        }

        return false;
    }

    /// 05 �ǿ� ������ �����ϴ���?
    bool SuccessionExists()
    {
        // �ະ �˻�
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            if (SuccessionExistsInRow(r)) return true;
        }

        // ���� �˻�
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            if (SuccessionExistsInColumn(c)) return true;
        }

        return false;
    }
    
    /// [���� ���� ����]
    
    // �迭���� ���� ���ӿ�����Ʈ�� ����
    bool SwapElementsInArray(Coords selectedCoords, Coords swapCoords)
    {
        // ����ó��
        if (AreCoordsValidArrayMember(selectedCoords) == false) return false; // ������ ��ǥ�� �迭�� ��ȿ�� ��ǥ�� �ƴ�
        if (AreCoordsValidArrayMember(swapCoords) == false) return false; // �ٲٷ��� ��ǥ�� �迭�� ��ȿ�� ��ǥ�� �ƴ�

        if ((selectedCoords.row - swapCoords.row) * (selectedCoords.row - swapCoords.row)
            + (selectedCoords.col - swapCoords.col) * (selectedCoords.col - swapCoords.col) != 1)
            return false; // ������ ��ǥ�� �ٲٷ��� ��ǥ�� �������� ����

        // ����ó������ �հ� ������� �Դٸ� �ٲ���
        GameObject temp = elementArray[selectedCoords.row, selectedCoords.col];
        elementArray[selectedCoords.row, selectedCoords.col] = elementArray[swapCoords.row, swapCoords.col];
        elementArray[swapCoords.row, swapCoords.col] = temp;

        UpdateElementCoordsInfo();

        return true;
    }

    // �ܺο��� ��ǲ�� �޴¿뵵�� �Լ� "selectedCoords�� swapCoords�� �ٲ��ּ���"
    public bool ReceiveSwapInput(Coords selectedCoords, Coords swapCoords)
    {
        if (!canSwap) return false; // ������ ��ǲ�� �޴� ���°� �ƴմϴ�

        if (SwapElementsInArray(selectedCoords, swapCoords))
        {
            this.selectedCoords = selectedCoords;
            this.swapCoords = swapCoords;

            MoveVisualizers();

            canSwap = false;
            firstSwapActionsDone = false;

            return true;
        }
        else
        {
            return false;
        }
    }
    
    // ����׿�
    public GameObject latestSwappedCoordsVisualizer1;
    public GameObject latestSwappedCoordsVisualizer2;
    void MoveVisualizers()
    {
        latestSwappedCoordsVisualizer1.transform.position = new Vector3(selectedCoords.col, -selectedCoords.row, latestSwappedCoordsVisualizer1.transform.position.z);
        latestSwappedCoordsVisualizer2.transform.position = new Vector3(swapCoords.col, -swapCoords.row, latestSwappedCoordsVisualizer2.transform.position.z);
    }

    void ShowCoordsToPop(HashSet<Coords> coordsToPop)
    {
        foreach (Coords element in coordsToPop)
        {
            elementArray[element.row, element.col].GetComponent<ElementBase>().ShowAsTarget();
        }
    }

    HashSet<Coords> cp;

    void Update()
    {
        if (canSwap) return;

        if (!firstSwapActionsDone) // ���� ���� �˻��ؾ��ϴ� �͵�
        {
            /// ������ �˻�
            // ���ʸ� �������̸�?
            // �ΰ��� �������̸�?

            if (!SuccessionExists())
            {
                Debug.Log("NOTE: ��ȿ���� ���� ����");

                // �ٽ� �ٲ����
                bool result = SwapElementsInArray(selectedCoords, swapCoords);
                if (!result) Debug.Log("ERR: ��ȿ���� ���� �����ε� �ǵ������µ� �����ߴ�");

                canSwap = true;
            }

            firstSwapActionsDone = true;
        }
        else
        {
            while (SuccessionExists())
            {
                cp = GetSuccessiveCoordsInArr(); //B

                //N
                bool unusedItemsExist = true;
                while (unusedItemsExist)
                {
                    unusedItemsExist = AddItemEffectsToHashSet(cp);
                }

                //M
                ClearAndPushNew(cp);
            }

            UpdateElementCoordsInfo();
            canSwap = true;
        }
    }
}