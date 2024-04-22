using UnityEditor.Experimental.GraphView;
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

    // 테스트용
    public GameObject targetSignPrefab;
    private GameObject targetSignObject;

    /// 원소의 정보
    public ArrayCoords arrayCoords;
    public int type;

    // 아이템인지 여부
    public AttachableItem attachedItemType = AttachableItem.NONE;
    private GameObject attachedItemObject;

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

        if (type < elementSprites.Length) // 일반 원소
        {
            spriteRenderer.sprite = elementSprites[type];
        }
        else if (type == elementSprites.Length) // 롤리팝
        {
            spriteRenderer.sprite = lollipopSprite;
        }
        else if (type == elementSprites.Length + 1) // 젤리빈
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

    // 테스트용
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
}