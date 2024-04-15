using System.Collections.Generic;
using UnityEngine;

//struct Element
//{
//    public int type;
//}

//class ConsolePang
//{
//    private Element[,] myArray;
//    private int elementTypes; // 몇 종류의 원소들이 등장할 것인가? 애니팡류 게임들은 보통 4~5종류다

//    // 생성자
//    public ConsolePang(int rowNum, int colNum, int elementTypes) // 콘솔팡의 크기와 등장종류개수를 설정
//    {
//        myArray = new Element[rowNum, colNum];
//        this.elementTypes = elementTypes;

//        ShuffleArr();
//    }

//    // 연산자 오버로딩
//    public Element this[int r, int c]
//    {
//        get { return myArray[r, c]; }
//    }

//    public int GetRowNum() { return myArray.GetLength(0); }
//    public int GetColumnNum() { return myArray.GetLength(1); }

//    /// II. 종류 랜덤셔플
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

//    /// III. [row, col]의 원소를 인접한 방향에 있는 원소와 바꿈
//    public bool SwapElements(int row, int col, Direction dir)
//    {
//        // 예외처리들
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

//        // 예외처리들을 뚫고 여기까지 왔다면 바꾸자
//        Element temp = myArray[row, col];
//        myArray[row, col] = myArray[swapIndexRow, swapIndexCol];
//        myArray[swapIndexRow, swapIndexCol] = temp;

//        return true;
//    }

//    /// IV. 배열의 모든 3개이상 반복원소들을 검출

//    // 도우미 함수 i. baseIndex부터 succession 개수만큼의 좌표들을 해시셋에 추가
//    private void AddCoordsToHashSet(HashSet<Coords> mySet, bool isRowTest, int currentRowOrCol, int baseIndex, int succession)
//    {
//        for (int i = baseIndex; i <= baseIndex + succession; i++)
//        {
//            Coords newCoord;

//            if (isRowTest) // 행별 검사시
//            {
//                newCoord.row = currentRowOrCol; // 현재 행
//                newCoord.col = i;
//            }
//            else // 열별 검사시
//            {
//                newCoord.row = i;
//                newCoord.col = currentRowOrCol; // 현재 열
//            }

//            mySet.Add(newCoord);
//        }
//    }

//    // 도우미 함수 ii. 행별 모든 연속을 검출해서 해시셋에 추가
//    private void AddRowSuccessionToHashSet(HashSet<Coords> mySet, int rowIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[rowIndex, 0].type;
//        int succession = 0;

//        for (int c = 1; c < myArray.GetLength(1); c++)
//        {
//            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
//            if (myArray[rowIndex, c].type == currentBase)
//            {
//                succession++;
//            }
//            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
//            else
//            {
//                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
//                if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
//                baseIndex = c;
//                currentBase = myArray[rowIndex, c].type;
//                succession = 0;
//            }
//        }
//        // 행 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
//        if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
//    }

//    // 도우미 함수 iii. 열별 모든 연속을 검출해서 해시셋에 추가
//    private void AddColumnSuccessionToHashSet(HashSet<Coords> mySet, int columnIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[0, columnIndex].type;
//        int succession = 0;

//        for (int r = 1; r < myArray.GetLength(0); r++)
//        {
//            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
//            if (myArray[r, columnIndex].type == currentBase)
//            {
//                succession++;
//            }
//            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
//            else
//            {
//                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
//                if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
//                baseIndex = r;
//                currentBase = myArray[r, columnIndex].type;
//                succession = 0;
//            }
//        }
//        // 열 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
//        if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
//    }

//    // 배열에 있는 모든 연속 검출
//    public HashSet<Coords> SearchSuccessionInArr()
//    {
//        HashSet<Coords> successiveCoords = new HashSet<Coords>();

//        // 1. 행별 검사
//        for (int i = 0; i < myArray.GetLength(0); i++)
//            AddRowSuccessionToHashSet(successiveCoords, i);

//        // 2. 열별 검사
//        for (int i = 0; i < myArray.GetLength(1); i++)
//            AddColumnSuccessionToHashSet(successiveCoords, i);

//        return successiveCoords;
//    }

//    /// V. 입력받은 자리의 원소들을 지우고 남은 원소들을 아래로 내려보낸 후, 위의 빈자리는 새 랜덤 원소로 채움
//    public void ClearAndPushNew(HashSet<Coords> coordsToPop)
//    {
//        if (coordsToPop.Count == 0) return;

//        for (int c = 0; c < myArray.GetLength(1); c++)
//        {
//            // 입력된 해시셋에서 열이 일치하는 원소들을 찾아 지워야할 행 정보를 모음
//            List<int> rowToErase = new List<int>();
//            foreach (Coords element in coordsToPop)
//            {
//                if (element.col == c) rowToErase.Add(element.row);
//            }
//            if (rowToErase.Count == 0) continue;

//            // 지우지 않고 남아있는 원소들을 열 아래쪽으로 밀어내리기
//            // int emptySpace에 밀어내려야할 칸수를 기록하면서 열의 높은 인덱스에서 낮은 인덱스까지 방문하며 아래부터 쌓아가는 식으로 위치를 재배정
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

//            // 이제 위에서부터 rowToErase.Count개의 자리는 죽은 원소들이므로 새로운 랜덤 원소들로 채워 줌
//            for (int r = 0; r < rowToErase.Count; r++)
//            {
//                Element newElement = new Element();
//                newElement.type = Random.Range(0, elementTypes);
//                myArray[r, c] = newElement;
//            }
//        }
//    }

//    /// VI. 한번의 스왑으로 연속이 생길수 있나 판별하는 함수 (아직 테스트 안해봄)

//    // 도우미 함수 i. 행별 연속 존재여부 검출
//    private bool SuccessionExistsInRow(int rowIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[rowIndex, 0].type;
//        int succession = 0;

//        for (int c = 1; c < myArray.GetLength(1); c++)
//        {
//            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
//            if (myArray[rowIndex, c].type == currentBase)
//            {
//                succession++;
//            }
//            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
//            else
//            {
//                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 return true
//                if (succession >= 2) return true;
//                baseIndex = c;
//                currentBase = myArray[rowIndex, c].type;
//                succession = 0;
//            }
//        }
//        // 행 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
//        if (succession >= 2) return true;

//        return false;
//    }

//    // 도우미 함수 ii. 열별 연속 존재여부 검출
//    private bool SuccessionExistsInColumn(int columnIndex)
//    {
//        int baseIndex = 0;
//        int currentBase = myArray[0, columnIndex].type;
//        int succession = 0;

//        for (int r = 1; r < myArray.GetLength(0); r++)
//        {
//            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
//            if (myArray[r, columnIndex].type == currentBase)
//            {
//                succession++;
//            }
//            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
//            else
//            {
//                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 return true
//                if (succession >= 2) return true;
//                baseIndex = r;
//                currentBase = myArray[r, columnIndex].type;
//                succession = 0;
//            }
//        }
//        // 열 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
//        if (succession >= 2) return true;

//        return false;
//    }

//    // 전체 배열에서 한 번의 스왑으로 연속이 생길수 있는지 여부를 리턴하는 함수
//    public bool SwappingMakesSuccession()
//    {
//        // 할수있는 가로스왑을 다해보자
//        for (int r = 0; r < myArray.GetLength(0); r++)
//        {
//            for (int c = 0; c < myArray.GetLength(1) - 1; c++)
//            {
//                SwapElements(r, c, Direction.RIGHT);

//                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
//                {
//                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
//                    SwapElements(r, c, Direction.RIGHT);
//                    return true;
//                }
//                // 못찾아도 되돌려 놓기
//                SwapElements(r, c, Direction.RIGHT);
//            }
//        }

//        // 할수있는 세로스왑을 다해보자
//        for (int c = 0; c < myArray.GetLength(1); c++)
//        {
//            for (int r = 0; r < myArray.GetLength(0) - 1; r++)
//            {
//                SwapElements(r, c, Direction.DOWN);

//                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
//                {
//                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
//                    SwapElements(r, c, Direction.DOWN);
//                    return true;
//                }
//                // 못찾아도 되돌려 놓기
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
