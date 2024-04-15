using UnityEngine;

/// 이들은 유니팡매니저가 시키는대로 움직일 뿐, 유니팡매니저를 모름

public struct ArrayCoords
{
    public int row;
    public int col;

    public ArrayCoords(int r, int c)
    {
        row = r;
        col = c;
    }
}

public class IndividualElement_2 : MonoBehaviour
{
    // Component
    SpriteRenderer spriteRenderer;
    public Sprite[] sprites;

    /// 원소의 정보
    public ArrayCoords arrayCoords;
    public int type;

    // 아이템인지 여부
    public bool isRowClearItem;
    public bool isColumnClearItem;
    public bool isBomb;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        //// 자기 자리로 가기
        //transform.position = new Vector3(arrayCoords.col, -arrayCoords.row);
    }

    public void MoveTo(int row, int col)
    {
        arrayCoords.row = row;
        arrayCoords.col = col;

        transform.position = new Vector3(col, -row);
    }

    public void SetType(int n)
    {
        type = n;

        // 해당 타입에 맞는 스프라이트로 갈아끼우기
        if (type < 5)
        {
            spriteRenderer.sprite = sprites[type];
        }
        else
        {
            spriteRenderer.sprite = null;
        }
    }
}