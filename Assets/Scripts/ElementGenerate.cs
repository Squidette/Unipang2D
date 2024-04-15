using System.Collections.Generic;
using UnityEngine;

//struct Element
//{
//    public int type;
//}

//class ConsolePang
//{
//    private Element[,] myArray;
//    private int elementTypes; // �� ������ ���ҵ��� ������ ���ΰ�? �ִ��η� ���ӵ��� ���� 4~5������

//    // ������
//    public ConsolePang(int rowNum, int colNum, int elementTypes) // �ܼ����� ũ��� �������������� ����
//    {
//        myArray = new Element[rowNum, colNum];
//        this.elementTypes = elementTypes;

//        ShuffleArr();
//    }

//    // ������ �����ε�
//    public Element this[int r, int c]
//    {
//        get { return myArray[r, c]; }
//    }

//    public int GetRowNum() { return myArray.GetLength(0); }
//    public int GetColumnNum() { return myArray.GetLength(1); }

//    /// II. ���� ��������
//    public void ShuffleArr()
//    {
//        for (int r = 0; r < myArray.GetLength(0); r++)
//        {
//            for (int c = 0; c < myArray.GetLength(1); c++)
//            {
//                myArray[r, c].type = Random.Range(0, elementTypes);
//            }
//        }
//    }

//    /// III. [row, col]�� ���Ҹ� ������ ���⿡ �ִ� ���ҿ� �ٲ�
//    public bool SwapElements(int row, int col, Direction dir)
//    {
//        // ����ó����
//        if (row < 0 || row >= myArray.GetLength(0)) return false;
//        if (col < 0 || col >= myArray.GetLength(1)) return false;

//        int swapIndexRow = row;
//        int swapIndexCol = col;

//        switch (dir)
//        {
//            case Direction.UP:
//                swapIndexRow--;
//                break;
//            case Direction.DOWN:
//                swapIndexRow++;
//                break;
//            case Direction.LEFT:
//                swapIndexCol--;
//                break;
//            case Direction.RIGHT:
//                swapIndexCol++;
//                break;
//        }

//        if (swapIndexRow < 0 || swapIndexRow >= myArray.GetLength(0)) return false;
//        if (swapIndexCol < 0 || swapIndexCol >= myArray.GetLength(1)) return false;

//        // ����ó������ �հ� ������� �Դٸ� �ٲ���
//        Element temp = myArray[row, col];
//        myArray[row, col] = myArray[swapIndexRow, swapIndexCol];
//        myArray[swapIndexRow, swapIndexCol] = temp;

//        return true;
//    }

//    /// IV. �迭�� ��� 3���̻� �ݺ����ҵ��� ����

//    // ����� �Լ� i. baseIndex���� succession ������ŭ�� ��ǥ���� �ؽü¿� �߰�
//    private void AddCoordsToHashSet(HashSet<Coords> mySet, bool isRowTest, int currentRowOrCol, int baseIndex, int succession)
//    {
//        for (int i = baseIndex; i <= baseIndex + succession; i++)
//        {
//            Coords newCoord;

//            if (isRowTest) // �ະ �˻��
//            {
//                newCoord.row = currentRowOrCol; // ���� ��
//                newCoord.col = i;
//            }
//            else // ���� �˻��
//            {
//                newCoord.row = i;
//                newCoord.col = currentRowOrCol; // ���� ��
//            }

//            mySet.Add(newCoord);
//        }
//    }

//    // ����� �Լ� ii. �ະ ��� ������ �����ؼ� �ؽü¿� �߰�
//    private void AddRowSuccessionToHashSet(HashSet<Coords> mySet, int rowIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[rowIndex, 0].type;
//        int succession = 0;

//        for (int c = 1; c < myArray.GetLength(1); c++)
//        {
//            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
//            if (myArray[rowIndex, c].type == currentBase)
//            {
//                succession++;
//            }
//            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
//            else
//            {
//                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� �ش� ������ �ؽü¿� �߰�
//                if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
//                baseIndex = c;
//                currentBase = myArray[rowIndex, c].type;
//                succession = 0;
//            }
//        }
//        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
//        if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
//    }

//    // ����� �Լ� iii. ���� ��� ������ �����ؼ� �ؽü¿� �߰�
//    private void AddColumnSuccessionToHashSet(HashSet<Coords> mySet, int columnIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[0, columnIndex].type;
//        int succession = 0;

//        for (int r = 1; r < myArray.GetLength(0); r++)
//        {
//            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
//            if (myArray[r, columnIndex].type == currentBase)
//            {
//                succession++;
//            }
//            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
//            else
//            {
//                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� �ش� ������ �ؽü¿� �߰�
//                if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
//                baseIndex = r;
//                currentBase = myArray[r, columnIndex].type;
//                succession = 0;
//            }
//        }
//        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
//        if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
//    }

//    // �迭�� �ִ� ��� ���� ����
//    public HashSet<Coords> SearchSuccessionInArr()
//    {
//        HashSet<Coords> successiveCoords = new HashSet<Coords>();

//        // 1. �ະ �˻�
//        for (int i = 0; i < myArray.GetLength(0); i++)
//            AddRowSuccessionToHashSet(successiveCoords, i);

//        // 2. ���� �˻�
//        for (int i = 0; i < myArray.GetLength(1); i++)
//            AddColumnSuccessionToHashSet(successiveCoords, i);

//        return successiveCoords;
//    }

//    /// V. �Է¹��� �ڸ��� ���ҵ��� ����� ���� ���ҵ��� �Ʒ��� �������� ��, ���� ���ڸ��� �� ���� ���ҷ� ä��
//    public void ClearAndPushNew(HashSet<Coords> coordsToPop)
//    {
//        if (coordsToPop.Count == 0) return;

//        for (int c = 0; c < myArray.GetLength(1); c++)
//        {
//            // �Էµ� �ؽü¿��� ���� ��ġ�ϴ� ���ҵ��� ã�� �������� �� ������ ����
//            List<int> rowToErase = new List<int>();
//            foreach (Coords element in coordsToPop)
//            {
//                if (element.col == c) rowToErase.Add(element.row);
//            }
//            if (rowToErase.Count == 0) continue;

//            // ������ �ʰ� �����ִ� ���ҵ��� �� �Ʒ������� �о����
//            // int emptySpace�� �о������ ĭ���� ����ϸ鼭 ���� ���� �ε������� ���� �ε������� �湮�ϸ� �Ʒ����� �׾ư��� ������ ��ġ�� �����
//            int emptySpace = 0;
//            for (int r = myArray.GetLength(0) - 1; r >= 0; r--)
//            {
//                if (rowToErase.Contains(r))
//                {
//                    emptySpace++;
//                }
//                else
//                {
//                    if (emptySpace != 0)
//                    {
//                        myArray[r + emptySpace, c] = myArray[r, c];
//                    }
//                }
//            }

//            // ���� ���������� rowToErase.Count���� �ڸ��� ���� ���ҵ��̹Ƿ� ���ο� ���� ���ҵ�� ä�� ��
//            for (int r = 0; r < rowToErase.Count; r++)
//            {
//                Element newElement = new Element();
//                newElement.type = Random.Range(0, elementTypes);
//                myArray[r, c] = newElement;
//            }
//        }
//    }

//    /// VI. �ѹ��� �������� ������ ����� �ֳ� �Ǻ��ϴ� �Լ� (���� �׽�Ʈ ���غ�)

//    // ����� �Լ� i. �ະ ���� ���翩�� ����
//    private bool SuccessionExistsInRow(int rowIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[rowIndex, 0].type;
//        int succession = 0;

//        for (int c = 1; c < myArray.GetLength(1); c++)
//        {
//            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
//            if (myArray[rowIndex, c].type == currentBase)
//            {
//                succession++;
//            }
//            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
//            else
//            {
//                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� return true
//                if (succession >= 2) return true;
//                baseIndex = c;
//                currentBase = myArray[rowIndex, c].type;
//                succession = 0;
//            }
//        }
//        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
//        if (succession >= 2) return true;

//        return false;
//    }

//    // ����� �Լ� ii. ���� ���� ���翩�� ����
//    private bool SuccessionExistsInColumn(int columnIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[0, columnIndex].type;
//        int succession = 0;

//        for (int r = 1; r < myArray.GetLength(0); r++)
//        {
//            // �����Ͱ� ���� Ÿ���� ������ ���� ī��Ʈ(succession)�� �ö�
//            if (myArray[r, columnIndex].type == currentBase)
//            {
//                succession++;
//            }
//            // �����Ͱ� �ٸ� Ÿ���� ������ ���� ī��Ʈ�� �ʱ�ȭ
//            else
//            {
//                // ����, ���ݱ��� ���� �ִ� ī��Ʈ�� 3����(succession = 2) �̻��̾����� return true
//                if (succession >= 2) return true;
//                baseIndex = r;
//                currentBase = myArray[r, columnIndex].type;
//                succession = 0;
//            }
//        }
//        // �� �˻簡 ������ ������ ������ ���ҵ��� �ݺ��Ǵ��� Ȯ������� �Ѵ�
//        if (succession >= 2) return true;

//        return false;
//    }

//    // ��ü �迭���� �� ���� �������� ������ ����� �ִ��� ���θ� �����ϴ� �Լ�
//    public bool SwappingMakesSuccession()
//    {
//        // �Ҽ��ִ� ���ν����� ���غ���
//        for (int r = 0; r < myArray.GetLength(0); r++)
//        {
//            for (int c = 0; c < myArray.GetLength(1) - 1; c++)
//            {
//                SwapElements(r, c, Direction.RIGHT);

//                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
//                {
//                    // ã���� �迭�� ������� �ǵ��� ���� return true
//                    SwapElements(r, c, Direction.RIGHT);
//                    return true;
//                }
//                // ��ã�Ƶ� �ǵ��� ����
//                SwapElements(r, c, Direction.RIGHT);
//            }
//        }

//        // �Ҽ��ִ� ���ν����� ���غ���
//        for (int c = 0; c < myArray.GetLength(1); c++)
//        {
//            for (int r = 0; r < myArray.GetLength(0) - 1; r++)
//            {
//                SwapElements(r, c, Direction.DOWN);

//                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
//                {
//                    // ã���� �迭�� ������� �ǵ��� ���� return true
//                    SwapElements(r, c, Direction.DOWN);
//                    return true;
//                }
//                // ��ã�Ƶ� �ǵ��� ����
//                SwapElements(r, c, Direction.DOWN);
//            }
//        }

//        return false;
//    }
//}

public class ElementGenerate : MonoBehaviour
{
    //public GameObject daniellePrefab;
    //public GameObject haerinPrefab;
    //public GameObject haniPrefab;
    //public GameObject hyeinPrefab;
    //public GameObject minjiPrefab;

    //public Camera myCamera;

    //private GameObject[,] unipangArray = new GameObject[7, 7];

    //Vector3 mouseDownPosition;
    //Coords mouseDownCoords;

    //ConsolePang game;

    void Start()
    {
        //game = new ConsolePang(7, 7, 5);
        //game.ShuffleArr();

        //while (game.SearchSuccessionInArr().Count != 0)
        //{
        //    game.ClearAndPushNew(game.SearchSuccessionInArr());
        //}

        //for (int r = 0; r < game.GetRowNum(); r++)
        //{
        //    for (int c = 0; c < game.GetColumnNum(); c++)
        //    {
        //        switch (game[r, c].type)
        //        {
        //            case 0:
        //                unipangArray[r, c] = Instantiate(daniellePrefab, new Vector3(c, -r, 0), Quaternion.identity);
        //                break;
        //            case 1:
        //                unipangArray[r, c] = Instantiate(haerinPrefab, new Vector3(c, -r, 0), Quaternion.identity);
        //                break;
        //            case 2:
        //                unipangArray[r, c] = Instantiate(haniPrefab, new Vector3(c, -r, 0), Quaternion.identity);
        //                break;
        //            case 3:
        //                unipangArray[r, c] = Instantiate(hyeinPrefab, new Vector3(c, -r, 0), Quaternion.identity);
        //                break;
        //            case 4:
        //                unipangArray[r, c] = Instantiate(minjiPrefab, new Vector3(c, -r, 0), Quaternion.identity);
        //                break;
        //        }
        //        unipangArray[r, c].GetComponent<IndividualElement>().gameCoord = new Vector3(c, -r, 0);
        //    }
        //}
    }

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    mouseDownPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
        //    mouseDownCoords.row = -(int)Mathf.Round(mouseDownPosition.y);
        //    mouseDownCoords.col = (int)Mathf.Round(mouseDownPosition.x);
        //    Debug.Log($"GetMouseButtonDown: ({mouseDownPosition.x}, {mouseDownPosition.y})");
        //    Debug.Log($"GetMouseButtonDown: [{mouseDownCoords.row}, {mouseDownCoords.col}]");
        //}

        //if (0 <= mouseDownCoords.row && mouseDownCoords.row < 7 && 0 <= mouseDownCoords.col && mouseDownCoords.col < 7)
        //{

        //    if (Input.GetMouseButtonUp(0))
        //    {
        //        Vector3 mouseUpPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
        //        int mouseUpCoordsRow = -(int)Mathf.Round(mouseUpPosition.y);
        //        int mouseUpCoordsColumn = (int)Mathf.Round(mouseUpPosition.x);
        //        Debug.Log($"GetMouseButtonUp: ({mouseUpPosition.x}, {mouseUpPosition.y})");
        //        Debug.Log($"GetMouseButtonUp: [{mouseUpCoordsRow}, {mouseUpCoordsColumn}]");

        //        // UP
        //        if (mouseUpCoordsRow == mouseDownCoords.row - 1 && mouseUpCoordsColumn == mouseDownCoords.col)
        //        {
        //            if (game.SwapElements(mouseDownCoords.row, mouseDownCoords.col, Direction.UP))
        //            {
        //                Vector3 temp = unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord = unipangArray[mouseDownCoords.row - 1, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row - 1, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord = temp;
        //            }
        //        }

        //        // DOWN
        //        if (mouseUpCoordsRow == mouseDownCoords.row + 1 && mouseUpCoordsColumn == mouseDownCoords.col)
        //        {
        //            if (game.SwapElements(mouseDownCoords.row, mouseDownCoords.col, Direction.DOWN))
        //            {
        //                Vector3 temp = unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord = unipangArray[mouseDownCoords.row + 1, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row + 1, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord = temp;
        //            }
        //        }

        //        // RIGHT
        //        if (mouseUpCoordsRow == mouseDownCoords.row && mouseUpCoordsColumn == mouseDownCoords.col + 1)
        //        {
        //            if (game.SwapElements(mouseDownCoords.row, mouseDownCoords.col, Direction.RIGHT))
        //            {
        //                Vector3 temp = unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord = unipangArray[mouseDownCoords.row, mouseDownCoords.col + 1].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row, mouseDownCoords.col + 1].GetComponent<IndividualElement>().gameCoord = temp;
        //            }
        //        }

        //        // LEFT
        //        if (mouseUpCoordsRow == mouseDownCoords.row && mouseUpCoordsColumn == mouseDownCoords.col - 1)
        //        {
        //            if (game.SwapElements(mouseDownCoords.row, mouseDownCoords.col, Direction.LEFT))
        //            {
        //                Vector3 temp = unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row, mouseDownCoords.col].GetComponent<IndividualElement>().gameCoord = unipangArray[mouseDownCoords.row, mouseDownCoords.col - 1].GetComponent<IndividualElement>().gameCoord;
        //                unipangArray[mouseDownCoords.row, mouseDownCoords.col - 1].GetComponent<IndividualElement>().gameCoord = temp;
        //            }
        //        }
        //    }
        //}
    }
}
