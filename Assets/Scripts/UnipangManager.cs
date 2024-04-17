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

    /// 게임 초기화 관련
    
    // 원소의 종류 개수
    public int numOfElementTypes;

    // 게임의 행렬 수를 지정
    public int unipangGameRow;
    public int unipangGameCol;

    // 입력 관련
    Coords latestClickedCoords;

    Coords latestSwappedCoords;
    Direction latestSwappedDirection;

    /// 유니팡 행렬
    private GameObject[,] elementArray;
    //private GameObject[,] gridArray; // 원소가 아닌 그리드 특정적인 아이템 정보를 담는다

    void Start()
    {
        myCamera.transform.position = new Vector3((float)unipangGameCol / 2 - 0.5F, -(float)unipangGameRow / 2 + 0.5F, myCamera.transform.position.z);
        myCamera.orthographicSize = unipangGameCol;
        elementArray = new GameObject[unipangGameRow, unipangGameCol];

        latestSwappedCoords.row = -1;
        latestSwappedCoords.col = -1;

        // 초기화 랜덤 배정
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1); c++)
            {
                // 원소 오브젝트 생성
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // 타입 지정하기
                element.GetComponent<ElementBase>().SetType(UnityEngine.Random.Range(0, numOfElementTypes)); // 종류 +1은 롤리팝, +2는 젤리빈이다

                elementArray[r, c] = element;
            }
        }

        while (GetSuccessiveCoordsInArr().Count != 0)
        {
            ClearAndPushNew(GetSuccessiveCoordsInArr());
        }

        UpdateElementCoordsInfo();
    }

    /// [Visual Effects] 연산된 행렬 위치 정보를 행렬내 원소들에게 알려주고 해당 위치로 이동하게 합니다
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

    /// [유니팡 핵심 연산]

    /// 01 배열의 모든 3개이상 반복원소들을 검출 + 아이템 검출
    void AddCoordsToHashSet_Row(HashSet<Coords> mySet, int currentRow, int baseIndex, int succession) // succession = 2는 3연속, succession = 3은 4연속, ...
    {
        for (int i = baseIndex; i <= baseIndex + succession; i++)
        {
            Coords newCoord;
            newCoord.row = currentRow;
            newCoord.col = i;

            if (mySet.Contains(newCoord))
            {
                // 폭탄 아이템 장착
                elementArray[currentRow, i].GetComponent<ElementBase>().AttachItem(AttachableItem.BOMB);
                mySet.Remove(newCoord); // 처음 폭탄이 된 원소는 안 사라지고 남아 있음
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
                // 폭탄 아이템 장착
                elementArray[i, currentColumn].GetComponent<ElementBase>().AttachItem(AttachableItem.BOMB);
                mySet.Remove(newCoord); // 처음 폭탄이 된 원소는 안 사라지고 남아 있음
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
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (elementArray[rowIndex, c].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Row(mySet, rowIndex, baseIndex, succession);
                baseIndex = c;
                currentBase = elementArray[rowIndex, c].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // 행 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Row(mySet, rowIndex, baseIndex, succession);
    }
    void AddColumnSuccessionToHashSet(HashSet<Coords> mySet, int columnIndex)
    {
        int baseIndex = 0;
        int currentBase = elementArray[0, columnIndex].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int r = 1; r < elementArray.GetLength(0); r++)
        {
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (elementArray[r, columnIndex].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Column(mySet, columnIndex, baseIndex, succession);
                baseIndex = r;
                currentBase = elementArray[r, columnIndex].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // 열 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSet_Column(mySet, columnIndex, baseIndex, succession);
    }
    HashSet<Coords> GetSuccessiveCoordsInArr()
    {
        HashSet<Coords> successiveCoords = new HashSet<Coords>();

        // 1. 행별 검사
        for (int r = 0; r < elementArray.GetLength(0); r++)
            AddRowSuccessionToHashSet(successiveCoords, r);

        // 2. 열별 검사
        for (int c = 0; c < elementArray.GetLength(1); c++)
            AddColumnSuccessionToHashSet(successiveCoords, c);

        return successiveCoords;
    }

    /// 02 입력받은 자리의 원소들을 지우고 남은 원소들을 아래로 내려보낸 후, 위의 빈자리는 새 랜덤 원소로 채움
    void ClearAndPushNew(HashSet<Coords> coordsToPop)
    {
        if (coordsToPop.Count == 0) return;

        for (int c = 0; c < elementArray.GetLength(1); c++)
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

            // 이제 위에서부터 rowToErase.Count개의 자리는 죽은 원소들이므로 새로운 랜덤 원소들로 채워 줌
            for (int r = 0; r < rowToErase.Count; r++)
            {
                // 원소 오브젝트 생성
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);

                // 타입 지정하기
                element.GetComponent<ElementBase>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                // 배열에 추가하기
                Destroy(elementArray[r, c]);
                elementArray[r, c] = element;

                // 해당 위치로 옮기기
                element.GetComponent<ElementBase>().MoveTo(r, c);
            }
        }
    }

    /// 03 한번의 스왑으로 연속이 생길수 있나 판별하는 함수 (아직 테스트 안해봄)
    bool SuccessionExistsInRow(int rowIndex)
    {
        int baseIndex = 0;
        int currentBase = elementArray[rowIndex, 0].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int c = 1; c < elementArray.GetLength(1); c++)
        {
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (elementArray[rowIndex, c].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 return true
                if (succession >= 2) return true;
                baseIndex = c;
                currentBase = elementArray[rowIndex, c].GetComponent<ElementBase>().type;
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
        int currentBase = elementArray[0, columnIndex].GetComponent<ElementBase>().type;
        int succession = 0;

        for (int r = 1; r < elementArray.GetLength(0); r++)
        {
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (elementArray[r, columnIndex].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 return true
                if (succession >= 2) return true;
                baseIndex = r;
                currentBase = elementArray[r, columnIndex].GetComponent<ElementBase>().type;
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
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1) - 1; c++)
            {
                SwapElementsInArray(r, c, Direction.RIGHT);

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
                    SwapElementsInArray(r, c, Direction.RIGHT);
                    return true;
                }
                // 못찾아도 되돌려 놓기
                SwapElementsInArray(r, c, Direction.RIGHT);
            }
        }

        // 할수있는 세로스왑을 다해보자
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            for (int r = 0; r < elementArray.GetLength(0) - 1; r++)
            {
                SwapElementsInArray(r, c, Direction.DOWN);

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
                    SwapElementsInArray(r, c, Direction.DOWN);
                    return true;
                }
                // 못찾아도 되돌려 놓기
                SwapElementsInArray(r, c, Direction.DOWN);
            }
        }

        return false;
    }

    /// [게임 진행 관련]
    bool SwapElementsInArray(int row, int col, Direction dir)
    {
        // 예외처리들
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

        // 예외처리들을 뚫고 여기까지 왔다면 바꾸자
        GameObject temp = elementArray[row, col];
        elementArray[row, col] = elementArray[swapIndexRow, swapIndexCol];
        elementArray[swapIndexRow, swapIndexCol] = temp;

        return true;
    }

    // 테스트
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
            // 지울 곳 검색하고 표시 + 아이템 장착
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
            // 표시한 것 지우기 + 터뜨려진 아이템 사용하기
            ClearAndPushNew(GetSuccessiveCoordsInArr());
            UpdateElementCoordsInfo();
        }

        // 드래그로 스왑 인풋 받기
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
