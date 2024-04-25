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

    /// 게임 초기화 관련

    // 원소의 종류 개수
    public int numOfElementTypes;

    // 게임의 행렬 수를 지정
    public int unipangGameRow;
    public int unipangGameCol;

    /// 게임 진행 관련
    Coords selectedCoords;
    Coords swapCoords;

    public bool canSwap;
    bool firstSwapActionsDone; // 첫 번째 스왑시에만 가능한 효과들을 발동시키고 꺼질 불변수

    /// 유니팡 행렬
    private GameObject[,] elementArray;
    //private GameObject[,] gridArray; // 원소가 아닌 그리드 특정적인 아이템 정보를 담는다

    void Start()
    {
        canSwap = true;
        firstSwapActionsDone = true;

        myCamera.transform.position = new Vector3((float)unipangGameCol / 2 - 0.5F, -(float)unipangGameRow / 2 + 0.5F, myCamera.transform.position.z);
        myCamera.orthographicSize = unipangGameCol;
        elementArray = new GameObject[unipangGameRow, unipangGameCol];

        // 초기화 랜덤 배정
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            for (int c = 0; c < elementArray.GetLength(1); c++)
            {
                GameObject element = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);
                element.transform.SetParent(transform);

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

    /// 01 연산된 행렬 위치 정보를 행렬내 원소들에게 알려주고 해당 위치로 이동하게 함
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

    /// [유니팡 핵심 연산]

    /// 01 배열의 모든 3개이상 반복원소들을 검출 + 아이템 검출 - 스왑후 한번 실행한다
    void AddCoordsToHashSetAndAttachItem_Row(HashSet<Coords> mySet, int currentRow, int baseIndex, int succession) // succession = 2는 3연속, succession = 3은 4연속, ...
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

            // 젤리빈 아이템 검출
            if (jellyBeanItemIndex != -1 && i == jellyBeanItemIndex)
            {
                elementArray[currentRow, i].GetComponent<ElementBase>().SetType(numOfElementTypes + 1);
                mySet.Remove(newCoord);
            }
            // 행 클리어 아이템 검출
            else if (rowClearItemIndex != -1 && i == rowClearItemIndex)
            {
                elementArray[currentRow, i].GetComponent<ElementBase>().AttachItem(AttachableItem.ROWCLEAR);
                mySet.Remove(newCoord);
            }
            // 폭탄 아이템 검출
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

    void AddCoordsToHashSetAndAttachItem_Column(HashSet<Coords> mySet, int currentColumn, int baseIndex, int succession) // succession = 2는 3연속, succession = 3은 4연속, ...
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

            // 젤리빈 아이템 검출
            if (jellyBeanItemIndex != -1 && i == jellyBeanItemIndex)
            {
                elementArray[i, currentColumn].GetComponent<ElementBase>().SetType(numOfElementTypes + 1);
                mySet.Remove(newCoord);
            }
            // 행 클리어 아이템 검출
            else if (columnClearItemIndex != -1 && i == columnClearItemIndex)
            {
                elementArray[i, currentColumn].GetComponent<ElementBase>().AttachItem(AttachableItem.COLUMNCLEAR);
                mySet.Remove(newCoord);
            }
            // 폭탄 아이템 검출
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
            // 이전것과 같은 타입이 있으면 연속 카운트(succession)가 올라감
            if (elementArray[rowIndex, c].GetComponent<ElementBase>().type == currentBase)
            {
                succession++;
            }
            // 이전것과 다른 타입을 만나면 연속 카운트를 초기화
            else
            {
                // 만약, 지금까지 세고 있던 카운트가 3연속(succession = 2) 이상이었으면 해당 정보를 해시셋에 추가
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Row(mySet, rowIndex, baseIndex, succession);
                baseIndex = c;
                currentBase = elementArray[rowIndex, c].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // 행 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Row(mySet, rowIndex, baseIndex, succession);
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
                if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Column(mySet, columnIndex, baseIndex, succession);
                baseIndex = r;
                currentBase = elementArray[r, columnIndex].GetComponent<ElementBase>().type;
                succession = 0;
            }
        }
        // 열 검사가 끝나고 나서도 마지막 원소들이 반복되는지 확인해줘야 한다
        if (succession >= 2 && currentBase < numOfElementTypes) AddCoordsToHashSetAndAttachItem_Column(mySet, columnIndex, baseIndex, succession);
    }

    HashSet<Coords> GetSuccessiveCoordsInArr() // <- 핵심 함수!
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

    /// 02 터지는 아이템의 효과 발동 - 더이상 추가되는 아이템이 없을 때까지 연속적으로 실행
    public bool AreCoordsValidArrayMember(Coords coords)
    {
        if (coords.row < 0) return false;
        if (elementArray.GetLength(0) <= coords.row) return false;
        if (coords.col < 0) return false;
        if (elementArray.GetLength(1) <= coords.col) return false;

        return true;
    }

    // 전달받은 좌표가 그리드 내의 유효한 좌표인지 확인하고 해당 좌표의 원소를 안전하게 해쉬셋에 추가
    void AddCoordsToHashSet_s(HashSet<Coords> mySet, Coords coords)
    {
        if (!AreCoordsValidArrayMember(coords)) return;
        if (elementArray[coords.row, coords.col].GetComponent<ElementBase>().type >= numOfElementTypes) return;

        mySet.Add(coords);
    }

    // 아이템이 터지면 효과를 발동해준다 (열 터뜨리기, 행 터뜨리기, 폭탄 터뜨리기) <- 핵심 함수!
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

    /// 03 입력받은 자리의 원소들을 지우고 남은 원소들을 아래로 내려보낸 후, 위의 빈자리는 새 랜덤 원소로 채움 - 아이템 효과를 전부 마친 후 한 번 실행한다
    void ClearAndPushNew(HashSet<Coords> coordsToPop) // <- 핵심 함수!
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
                element.transform.SetParent(transform);

                // 타입 지정하기
                element.GetComponent<ElementBase>().SetType(UnityEngine.Random.Range(0, numOfElementTypes));

                // 배열에 추가하기
                Destroy(elementArray[r, c]);
                elementArray[r, c] = element;

                // 해당 위치로 옮기기
                element.GetComponent<ElementBase>().MoveTo_Instant(new Coords(r, c));
            }
        }
    }

    /// 04 한번의 스왑으로 연속이 생길수 있나 판별하는 함수 (아직 테스트 안해봄)
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
                SwapElementsInArray(new Coords(r, c), new Coords(r, c + 1));

                if (SuccessionExistsInRow(r) || SuccessionExistsInColumn(c) || SuccessionExistsInColumn(c + 1))
                {
                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
                    SwapElementsInArray(new Coords(r, c), new Coords(r, c + 1));
                    return true;
                }
                // 못찾아도 되돌려 놓기
                SwapElementsInArray(new Coords(r, c), new Coords(r, c + 1));
            }
        }

        // 할수있는 세로스왑을 다해보자
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            for (int r = 0; r < elementArray.GetLength(0) - 1; r++)
            {
                SwapElementsInArray(new Coords(r, c), new Coords(r + 1, c));

                if (SuccessionExistsInColumn(c) || SuccessionExistsInRow(r) || SuccessionExistsInRow(r + 1))
                {
                    // 찾으면 배열을 원래대로 되돌려 놓고 return true
                    SwapElementsInArray(new Coords(r, c), new Coords(r + 1, c));
                    return true;
                }
                // 못찾아도 되돌려 놓기
                SwapElementsInArray(new Coords(r, c), new Coords(r + 1, c));
            }
        }

        return false;
    }

    /// 05 판에 연속이 존재하는지?
    bool SuccessionExists()
    {
        // 행별 검사
        for (int r = 0; r < elementArray.GetLength(0); r++)
        {
            if (SuccessionExistsInRow(r)) return true;
        }

        // 열별 검사
        for (int c = 0; c < elementArray.GetLength(1); c++)
        {
            if (SuccessionExistsInColumn(c)) return true;
        }

        return false;
    }
    
    /// [게임 진행 관련]
    
    // 배열상의 실제 게임오브젝트를 스왑
    bool SwapElementsInArray(Coords selectedCoords, Coords swapCoords)
    {
        // 예외처리
        if (AreCoordsValidArrayMember(selectedCoords) == false) return false; // 선택한 좌표가 배열내 유효한 좌표가 아님
        if (AreCoordsValidArrayMember(swapCoords) == false) return false; // 바꾸려는 좌표가 배열내 유효한 좌표가 아님

        if ((selectedCoords.row - swapCoords.row) * (selectedCoords.row - swapCoords.row)
            + (selectedCoords.col - swapCoords.col) * (selectedCoords.col - swapCoords.col) != 1)
            return false; // 선택한 좌표와 바꾸려는 좌표가 인접하지 않음

        // 예외처리들을 뚫고 여기까지 왔다면 바꾸자
        GameObject temp = elementArray[selectedCoords.row, selectedCoords.col];
        elementArray[selectedCoords.row, selectedCoords.col] = elementArray[swapCoords.row, swapCoords.col];
        elementArray[swapCoords.row, swapCoords.col] = temp;

        UpdateElementCoordsInfo();

        return true;
    }

    // 외부에서 인풋을 받는용도의 함수 "selectedCoords와 swapCoords를 바꿔주세요"
    public bool ReceiveSwapInput(Coords selectedCoords, Coords swapCoords)
    {
        if (!canSwap) return false; // 지금은 인풋을 받는 상태가 아닙니다

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
    
    // 디버그용
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

        if (!firstSwapActionsDone) // 스왑 직후 검사해야하는 것들
        {
            /// 젤리빈 검사
            // 한쪽만 젤리빈이면?
            // 두개다 젤리빈이면?

            if (!SuccessionExists())
            {
                Debug.Log("NOTE: 유효하지 않은 스왑");

                // 다시 바꿔놓기
                bool result = SwapElementsInArray(selectedCoords, swapCoords);
                if (!result) Debug.Log("ERR: 유효하지 않은 스왑인데 되돌려놓는데 실패했다");

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