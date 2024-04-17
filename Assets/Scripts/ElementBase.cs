using UnityEditor.Experimental.GraphView;
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

public enum AttachableItem
{
    ROWCLEAR,
    COLUMNCLEAR,
    BOMB,
    NONE
}

public class ElementBase : MonoBehaviour
{
    // Component
    SpriteRenderer spriteRenderer;

    public Sprite[] elementSprites;
    public Sprite jellyBeanSprite;
    public Sprite lollipopSprite;

    public GameObject rowClearItemPrefab;
    public GameObject columnClearItemPrefab;
    public GameObject bombItemPrefab;

    // �׽�Ʈ��
    public GameObject targetSignPrefab;
    private GameObject targetSignObject;

    /// ������ ����
    public ArrayCoords arrayCoords;
    public int type;

    // ���������� ����
    AttachableItem attachableItem = AttachableItem.NONE;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void MoveTo(int row, int col)
    {
        arrayCoords.row = row;
        arrayCoords.col = col;

        transform.position = new Vector3(col, -row, transform.position.z);
    }

    public void SetType(int n)
    {
        type = n;

        if (type < elementSprites.Length) // �Ϲ� ����
        {
            spriteRenderer.sprite = elementSprites[type];
        }
        else if (type == elementSprites.Length) // �Ѹ���
        {
            spriteRenderer.sprite = lollipopSprite;
        }
        else if (type == elementSprites.Length + 1) // ������
        {
            spriteRenderer.sprite = jellyBeanSprite;
        }
        else
        {
            spriteRenderer.sprite = null;
        }
    }

    public void AttachItem(AttachableItem i)
    {
        if (i == AttachableItem.NONE) return;
        if (attachableItem != AttachableItem.NONE) return;
        if (type >= elementSprites.Length) return;

        attachableItem = i;

        GameObject item = new GameObject();

        switch (attachableItem)
        {
            case AttachableItem.ROWCLEAR:
                item = Instantiate(rowClearItemPrefab, transform.position, Quaternion.identity);
                break;
            case AttachableItem.COLUMNCLEAR:
                item = Instantiate(columnClearItemPrefab, transform.position, Quaternion.identity);
                break;
            case AttachableItem.BOMB:
                item = Instantiate(bombItemPrefab, transform.position, Quaternion.identity);
                break;
        }

        item.transform.parent = transform;
    }

    // �׽�Ʈ��
    public void ShowAsTarget()
    {
        if (targetSignObject == null)
        {
            targetSignObject = new GameObject();
            targetSignObject = Instantiate(targetSignPrefab, transform.position, Quaternion.identity);
            targetSignObject.transform.parent = transform;
        }
        else
        {
            targetSignObject.SetActive(true);
        }
    }

    public void HideTarget()
    {
        if (targetSignObject != null)
        {
            targetSignObject.SetActive(false);
        }
    }
}