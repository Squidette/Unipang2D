using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
    Rigidbody2D rigid2D;

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

    // �۾����ٰ� �������
    bool isDwindling;
    float dwindleSpeed = 8.0F; // �� 1���� 1/8���� 0.125�ʸ��� ������°̴ϴ�
    float minScale = 0.01F;

    // ��������
    bool isFalling;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2D = GetComponent<Rigidbody2D>();
    }

    // �۾����� (������� ���� ��)
    public void ScaleDown(float time)
    {
        if (!spriteRenderer.enabled) return;
        isDwindling = true;
    }

    // �ڸ� ����
    public void AssignNewPosition(Coords newPosition)
    {
        positionInUnipang = newPosition;
    }

    // ����Ʈ ���� �� �̵�
    public void MoveToPosition_Instant()
    {
        transform.position = new Vector3(positionInUnipang.col, -positionInUnipang.row, transform.position.z);
    }

    // ��������
    public void MoveToPosition_FallDown(float time, int liftLength = 0)
    {
        transform.position = new Vector3(positionInUnipang.col, transform.position.y + liftLength, transform.position.z);

        if (isFalling) return;
        rigid2D.velocity = Vector3.zero;
        rigid2D.simulated = true;
        isFalling = true;
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
        if (isDwindling)
        {
            transform.localScale -= new Vector3(dwindleSpeed * Time.deltaTime, dwindleSpeed * Time.deltaTime, 1.0F);

            if (transform.localScale.x <= minScale)
            {
                transform.localScale = new Vector3(0.0F, 0.0F, 1.0F);
                spriteRenderer.enabled = false;
                isDwindling = false;
            }
        }

        if (isFalling)
        {
            if (transform.position.y <= -positionInUnipang.row)
            {
                rigid2D.simulated = false;
                isFalling = false;
                MoveToPosition_Instant();
            }
        }
    }
}