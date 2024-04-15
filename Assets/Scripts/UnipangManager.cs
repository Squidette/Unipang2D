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
    public int numOfElementTypes; // 원소 종류 개수

    public Camera myCamera;

    // 게임의 행렬 수를 지정합니다
    public int unipangGameRow;
    public int unipangGameCol;

    // 인풋
    Coords latestSelectedCoords;

    private GameObject[,] array;

    void Start()
    {
        array = new GameObject[unipangGameRow, unipangGameCol];

        // 초기화 랜덤 배정
        for (int r = 0; r < array.GetLength(0); r++)
        {
            for (int c = 0; c < array.GetLength(1); c++)
            {
                // 원소 오브젝트 생성
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // 타입 지정하기
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

    // 오브젝트들의 배열내 인덱스 정보를 프리팹들에게 전달하고 해당하는 게임상 위치로 옮겨줌
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
        // 예외처리들
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

        // 예외처리들을 뚫고 여기까지 왔다면 바꾸자
        GameObject temp = array[row, col];
        array[row, col] = array[swapIndexRow, swapIndexCol];
        array[swapIndexRow, swapIndexCol] = temp;

        return true;
    }

    /// 배열의 모든 3개이상 반복원소들을 검출
    void AddCoordsToHashSet(HashSet<Coords> mySet, bool isRowTest, int currentRowOrCol, int baseIndex, int succession)
    {
        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;

            if (isRowTest) // 행별 검사시
            {
                newCoord.row = currentRowOrCol; // 현재 행
                newCoord.col = i;
            }
            else // 열별 검사시
            {
                newCoord.row = i;
                newCoord.col = currentRowOrCol; // 현재 열
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
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (array[rowIndex, c].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
                if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
                baseIndex = c;
                currentBase = array[rowIndex, c].GetComponent<IndividualElement_2>().type;
                succession = 0;
            }
        }
        // 행 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2) AddCoordsToHashSet(mySet, true, rowIndex, baseIndex, succession);
    }

    void AddColumnSuccessionToHashSet(HashSet<Coords> mySet, int columnIndex)
    {
        int baseIndex = 0;
        int currentBase = array[0, columnIndex].GetComponent<IndividualElement_2>().type;
        int succession = 0;

        for (int r = 1; r < array.GetLength(0); r++)
        {
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (array[r, columnIndex].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
                if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
                baseIndex = r;
                currentBase = array[r, columnIndex].GetComponent<IndividualElement_2>().type;
                succession = 0;
            }
        }
        // 열 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2) AddCoordsToHashSet(mySet, false, columnIndex, baseIndex, succession);
    }

    HashSet<Coords> GetSuccessiveCoordsInArr()
    {
        HashSet<Coords> successiveCoords = new HashSet<Coords>();

        // 1. 행별 검사
        for (int r = 0; r < array.GetLength(0); r++)
            AddRowSuccessionToHashSet(successiveCoords, r);

        // 2. 열별 검사
        for (int c = 0; c < array.GetLength(1); c++)
            AddColumnSuccessionToHashSet(successiveCoords, c);

        return successiveCoords;
    }

    /// 입력받은 자리의 원소들을 지우고 남은 원소들을 아래로 내려보낸 후, 위의 빈자리는 새 랜덤 원소로 채움
    void ClearAndPushNew(HashSet<Coords> coordsToPop)
    {
        if (coordsToPop.Count == 0) return;

        for (int c = 0; c < array.GetLength(1); c++)
        {
            // 입력된 해시셋에서 열이 일치하는 원소들을 찾아 지워야할 행 정보를 모음
            List<int> rowToErase = new List<int>();
            foreach (Coords element in coordsToPop)
            {
                if (element.col == c) rowToErase.Add(element.row);
            }
            if (rowToErase.Count == 0) continue;

            // 지워지지 않고 남아있는 원소들을 열 아래쪽으로 밀어내리기
            // int emptySpace에 밀어내려야할 칸수를 기록하면서 열의 높은 인덱스에서 낮은 인덱스까지 방문하며 아래부터 쌓아가는 식으로 위치를 재배정
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

            // 이제 위에서부터 rowToErase.Count개의 자리는 죽은 원소들이므로 새로운 랜덤 원소들로 채워 줌
            for (int r = 0; r < rowToErase.Count; r++)
            {
                // 원소 오브젝트 생성
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // 타입 지정하기
                element.GetComponent<IndividualElement_2>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                // 배열에 추가하기
                Destroy(array[r, c]);
                array[r, c] = element;

                // 해당 위치로 옮기기
                element.GetComponent<IndividualElement_2>().MoveTo(r, c);
            }
        }
    }

    /// 한번의 스왑으로 연속이 생길수 있나 판별하는 함수 (아직 테스트 안해봄)

    bool SuccessionExistsInRow(int rowIndex)
    {
        int baseIndex = 0;
        int currentBase = array[rowIndex, 0].GetComponent<IndividualElement_2>().type;
        int succession = 0;

        for (int c = 1; c < array.GetLength(1); c++)
        {
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (array[rowIndex, c].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 return true
                if (succession >= 2) return true;
                baseIndex = c;
                currentBase = array[rowIndex, c].GetComponent<IndividualElement_2>().type;
                succession = 0;
            }
        }
        // 행 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
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
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (array[r, columnIndex].GetComponent<IndividualElement_2>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 return true
                if (succession >= 2) return true;
                baseIndex = r;
                currentBase = array[r, columnIndex].GetComponent<IndividualElement_2>().type;
                succession = 0;
            }
        }
        // 열 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2) return true;

        return false;
    }

    bool SwappingMakesSuccession()
    {
        // 할수있는 가로스왑을 다해보자
        for (int r = 0; r < array.GetLength(0); r++)
        {
            for (int c = 0; c < array.GetLength(1) - 1; c++)
            {
                SwapElements(r, c, Direction.RIGHT);

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
                    SwapElements(r, c, Direction.RIGHT);
                    return true;
                }
                // 못찾아도 되돌려 놓기
                SwapElements(r, c, Direction.RIGHT);
            }
        }

        // 할수있는 세로스왑을 다해보자
        for (int c = 0; c < array.GetLength(1); c++)
        {
            for (int r = 0; r < array.GetLength(0) - 1; r++)
            {
                SwapElements(r, c, Direction.DOWN);

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
                    SwapElements(r, c, Direction.DOWN);
                    return true;
                }
                // 못찾아도 되돌려 놓기
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

        // 드래그로 스왑 인풋 받기
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
