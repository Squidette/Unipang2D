using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
    Rigidbody2D rigid2D;

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

    // 작아지다가 사라지기
    bool isDwindling;
    float dwindleSpeed = 8.0F; // 약 1초의 1/8인인 0.125초만에 사라지는겁니다
    float minScale = 0.01F;

    // 떨어지기
    bool isFalling;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2D = GetComponent<Rigidbody2D>();
    }

    // 작아지기 (사라지기 위한 것)
    public void ScaleDown(float time)
    {
        if (!spriteRenderer.enabled) return;
        isDwindling = true;
    }

    // 자리 지정
    public void AssignNewPosition(Coords newPosition)
    {
        positionInUnipang = newPosition;
    }

    // 이펙트 없이 뿅 이동
    public void MoveToPosition_Instant()
    {
        transform.position = new Vector3(positionInUnipang.col, -positionInUnipang.row, transform.position.z);
    }

    // 떨어지기
    public void MoveToPosition_FallDown(float time, int liftLength = 0)
    {
        transform.position = new Vector3(positionInUnipang.col, transform.position.y + liftLength, transform.position.z);

        if (isFalling) return;
        rigid2D.velocity = Vector3.zero;
        rigid2D.simulated = true;
        isFalling = true;
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