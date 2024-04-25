using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// 이들은 유니팡매니저가 시키는대로 움직일 뿐, 유니팡매니저를 모름

// 붙일 수 있는 아이템들의 목록
// * 젤리빈과 롤리팝은 일반 타입 원소들에 붙일 수 있는 아이템이 아닌, 별개 타입의 아이템으로 취급
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
    public Coords positionInUnipang;
    public int type;

    // 아이템인지 여부
    public AttachableItem attachedItemType = AttachableItem.NONE;
    private GameObject attachedItemObject;

    // 움직임 테스트

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 이펙트 없이 뿅 이동
    public void MoveTo_Instant(Coords coordsToMoveTo)
    {
        positionInUnipang = coordsToMoveTo;
        transform.position = new Vector3(positionInUnipang.col, -positionInUnipang.row, transform.position.z);
    }

    // 등속 직선 운동으로 이동

    public void SetType(int n) // 타입 지정하기
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

    public void AttachItem(AttachableItem i) // 아이템 붙이기
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

    // 디버깅용
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