using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// �̵��� �����θŴ����� ��Ű�´�� ������ ��, �����θŴ����� ��

// ���� �� �ִ� �����۵��� ���
// * ������� �Ѹ����� �Ϲ� Ÿ�� ���ҵ鿡 ���� �� �ִ� �������� �ƴ�, ���� Ÿ���� ���������� ���
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
    public Coords positionInUnipang;
    public int type;

    // ���������� ����
    public AttachableItem attachedItemType = AttachableItem.NONE;
    private GameObject attachedItemObject;

    // ������ �׽�Ʈ

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ����Ʈ ���� �� �̵�
    public void MoveTo_Instant(Coords coordsToMoveTo)
    {
        positionInUnipang = coordsToMoveTo;
        transform.position = new Vector3(positionInUnipang.col, -positionInUnipang.row, transform.position.z);
    }

    // ��� ���� ����� �̵�

    public void SetType(int n) // Ÿ�� �����ϱ�
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

    public void AttachItem(AttachableItem i) // ������ ���̱�
    {
        if (type >= elementSprites.Length) return;

        switch (i)
        {
            case AttachableItem.ROWCLEAR:
                if (attachedItemObject != null) Destroy(attachedItemObject);
                attachedItemObject = Instantiate(rowClearItemPrefab, transform.position, Quaternion.identity);
                attachedItemObject.transform.parent = transform;
                break;
            case AttachableItem.COLUMNCLEAR:
                if (attachedItemObject != null) Destroy(attachedItemObject);
                attachedItemObject = Instantiate(columnClearItemPrefab, transform.position, Quaternion.identity);
                attachedItemObject.transform.parent = transform;
                break;
            case AttachableItem.BOMB:
                if (attachedItemObject != null) Destroy(attachedItemObject);
                attachedItemObject = Instantiate(bombItemPrefab, transform.position, Quaternion.identity);
                attachedItemObject.transform.parent = transform;
                break;
            case AttachableItem.NONE:
                if (attachedItemObject != null) Destroy(attachedItemObject);
                break;
        }

        attachedItemType = i;
    }

    // ������
    public void ShowAsTarget()
    {
        if (targetSignObject == null)
        {
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

    void Update()
    {
    }
}