using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEditor;
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
    public GameObject elementPrefab;
    public int numOfElementTypes; // ���� ���� ����

    public Camera myCamera;

    // ������ ��� ���� �����մϴ�
    public int unipangGameRow;
    public int unipangGameCol;

    // ��ǲ
    Coords latestSelectedCoords;

    private GameObject[,] array;

    void Start()
    {
        array = new GameObject[unipangGameRow, unipangGameCol];

        // �ʱ�ȭ ���� ����
        for (int r = 0; r < array.GetLength(0); r++)
        {
            for (int c = 0; c < array.GetLength(1); c++)
            {
                // ���� ������Ʈ ����
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // Ÿ�� �����ϱ�
                element.GetComponent<IndividualElement_2>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                array[r, c] = element;
            }
        }

        while (GetSuccessiveCoordsInArr().Count != 0)
        {
            ClearAndPushNew(GetSuccessiveCoordsInArr());
        }
        UpdateElementCoordsInfo();
    }

    // ������Ʈ���� �迭�� �ε��� ������ �����յ鿡�� �����ϰ� �ش��ϴ� ���ӻ� ��ġ�� �Ű���
    void UpdateElementCoordsInfo()
    {
        for (int r = 0; r < array.GetLength(0); r++)
        {
            for (int c = 0; c < array.GetLength(1); c++)
            {
                array[r, c].GetComponent<IndividualElement_2>().MoveTo(r, c);
            }
        }
    }

    bool SwapElements(int row, int col, Direction dir)
    {
        // ����ó����
        if (row < 0 || row >= array.GetLength(0)) return false;
        if (col < 0 || col >= array.GetLength(1)) return false;

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

        if (swapIndexRow < 0 || swapIndexRow >= array.GetLength(0)) return false;
        if (swapIndexCol < 0 || swapIndexCol >= array.GetLength(1)) return false;

        // ����ó������ �հ� ������� �Դٸ� �ٲ���
        GameObject temp = array[row, col];
        array[row, col] = array[swapIndexRow, swapIndexCol];
        array[swapIndexRow, swapIndexCol] = temp;

        return true;
    }

    /// �迭�� ��� 3���̻� �ݺ����ҵ��� ����
    void AddCoordsToHashSet(HashSet<Coords> mySet, bool isRowTest, int currentRowOrCol, int baseIndex, int succession)
    {
        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;

            if (isRowTest) // �ະ �˻��
            {
                newCoord.row = currentRowOrCol; // ���� ��
                newCoord.col = i;
            }
            else // ���� �˻��
            {
                newCoord.row = i;
                newCoord.col = currentRowOrCol; // ���� ��
            }

            mySet.Add(newCoord);
        }
    }

    void AddRowSuccessionToHashSet(HashSet<Coords> mySet, int rowIndex)
    {
        int baseIndex = 0;
        int currentBase = array[rowIndex, 0].GetComponent<IndividualElement_2>().type;
        int succession = 0;

        for (int c = 1; c < array.GetLength(1); c++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (array[rowIndex, c].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� �ش� ������ �ؽü¿� �߰�
                if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
                baseIndex = c;
                currentBase = array[rowIndex, c].GetComponent<IndividualElement_2>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
    }

    void AddColumnSuccessionToHashSet(HashSet<Coords> mySet, int columnIndex)
    {
        int baseIndex = 0;
        int currentBase = array[0, columnIndex].GetComponent<IndividualElement_2>().type;
        int succession = 0;

        for (int r = 1; r < array.GetLength(0); r++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (array[r, columnIndex].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� �ش� ������ �ؽü¿� �߰�
                if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
                baseIndex = r;
                currentBase = array[r, columnIndex].GetComponent<IndividualElement_2>().type;
                succession = 0;
            }
        }
        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
        if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
    }

    HashSet<Coords> GetSuccessiveCoordsInArr()
    {
        HashSet<Coords> successiveCoords = new HashSet<Coords>();

        // 1. �ະ �˻�
        for (int r = 0; r < array.GetLength(0); r++)
            AddRowSuccessionToHashSet(successiveCoords, r);

        // 2. ���� �˻�
        for (int c = 0; c < array.GetLength(1); c++)
            AddColumnSuccessionToHashSet(successiveCoords, c);

        return successiveCoords;
    }

    /// �Է¹��� �ڸ��� ���ҵ��� ����� ���� ���ҵ��� �Ʒ��� �������� ��, ���� ���ڸ��� �� ���� ���ҷ� ä��
    void ClearAndPushNew(HashSet<Coords> coordsToPop)
    {
        if (coordsToPop.Count == 0) return;

        for (int c = 0; c < array.GetLength(1); c++)
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
            for (int r = array.GetLength(0) - 1; r >= 0; r--)
            {
                if (rowToErase.Contains(r))
                {
                    emptySpace++;
                }
                else
                {
                    if (emptySpace != 0)
                    {
                        GameObject temp = array[r, c];
                        array[r, c] = array[r + emptySpace, c];
                        array[r + emptySpace, c] = temp;
                    }
                }
            }

            // ���� ���������� rowToErase.Count���� �ڸ��� ���� ���ҵ��̹Ƿ� ���ο� ���� ���ҵ�� ä�� ��
            for (int r = 0; r < rowToErase.Count; r++)
            {
                // ���� ������Ʈ ����
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // Ÿ�� �����ϱ�
                element.GetComponent<IndividualElement_2>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                // �迭�� �߰��ϱ�
                Destroy(array[r, c]);
                array[r, c] = element;

                // �ش� ��ġ�� �ű��
                element.GetComponent<IndividualElement_2>().MoveTo(r, c);
            }
        }
    }

    /// �ѹ��� �������� ������ ����� �ֳ� �Ǻ��ϴ� �Լ� (���� �׽�Ʈ ���غ�)

    bool SuccessionExistsInRow(int rowIndex)
    {
        int baseIndex = 0;
        int currentBase = array[rowIndex, 0].GetComponent<IndividualElement_2>().type;
        int succession = 0;

        for (int c = 1; c < array.GetLength(1); c++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (array[rowIndex, c].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� return true
                if (succession >= 2) return true;
                baseIndex = c;
                currentBase = array[rowIndex, c].GetComponent<IndividualElement_2>().type;
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
        int currentBase = array[0, columnIndex].GetComponent<IndividualElement_2>().type;
        int succession = 0;

        for (int r = 1; r < array.GetLength(0); r++)
        {
            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
            if (array[r, columnIndex].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
            else
            {
                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� return true
                if (succession >= 2) return true;
                baseIndex = r;
                currentBase = array[r, columnIndex].GetComponent<IndividualElement_2>().type;
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
        for (int r = 0; r < array.GetLength(0); r++)
        {
            for (int c = 0; c < array.GetLength(1) - 1; c++)
            {
                SwapElements(r, c, Direction.RIGHT);

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElements(r, c, Direction.RIGHT);
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElements(r, c, Direction.RIGHT);
            }
        }

        // �Ҽ��ִ� ���ν����� ���غ���
        for (int c = 0; c < array.GetLength(1); c++)
        {
            for (int r = 0; r < array.GetLength(0) - 1; r++)
            {
                SwapElements(r, c, Direction.DOWN);

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // ã���� �迭�� ������� �ǵ��� ���� return true
                    SwapElements(r, c, Direction.DOWN);
                    return true;
                }
                // ��ã�Ƶ� �ǵ��� ����
                SwapElements(r, c, Direction.DOWN);
            }
        }

        return false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearAndPushNew(GetSuccessiveCoordsInArr());
            UpdateElementCoordsInfo();
        }

        // �巡�׷� ���� ��ǲ �ޱ�
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 latestMouseDownPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
            latestSelectedCoords.row = -(int)Mathf.Round(latestMouseDownPosition.y);
            latestSelectedCoords.col = (int)Mathf.Round(latestMouseDownPosition.x);
            Debug.Log($"latestSelectedCoords: [{latestSelectedCoords.row}, {latestSelectedCoords.col}]");
        }

        if (0 <= latestSelectedCoords.row && latestSelectedCoords.row < array.GetLength(0)
            && 0 <= latestSelectedCoords.col && latestSelectedCoords.col < array.GetLength(1))
        {
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mouseUpPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
                int rowToSwap = -(int)Mathf.Round(mouseUpPosition.y);
                int colToSwap = (int)Mathf.Round(mouseUpPosition.x);

                if (latestSelectedCoords.col == colToSwap)
                {
                    if (latestSelectedCoords.row > rowToSwap)
                    {
                        SwapElements(latestSelectedCoords.row, latestSelectedCoords.col, Direction.UP);
                    }
                    else if (latestSelectedCoords.row < rowToSwap)
                    {
                        SwapElements(latestSelectedCoords.row, latestSelectedCoords.col, Direction.DOWN);
                    }
                }
                else if (latestSelectedCoords.row == rowToSwap)
                {
                    if (latestSelectedCoords.col > colToSwap)
                    {
                        SwapElements(latestSelectedCoords.row, latestSelectedCoords.col, Direction.LEFT);
                    }
                    else if (latestSelectedCoords.col < colToSwap)
                    {
                        SwapElements(latestSelectedCoords.row, latestSelectedCoords.col, Direction.RIGHT);
                    }
                }
            }
            UpdateElementCoordsInfo();
        }
    }
}
