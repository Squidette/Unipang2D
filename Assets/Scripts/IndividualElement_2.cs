using UnityEngine;

/// �̵��� �����θŴ����� ��Ű�´�� ������ ��, �����θŴ����� ��

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

    /// ������ ����
    public ArrayCoords arrayCoords;
    public int type;

    // ���������� ����
    public bool isRowClearItem;
    public bool isColumnClearItem;
    public bool isBomb;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        //// �ڱ� �ڸ��� ����
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

        // �ش� Ÿ�Կ� �´� ��������Ʈ�� ���Ƴ����
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