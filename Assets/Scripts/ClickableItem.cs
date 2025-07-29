using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки Предмета")]
    public GamePhase itemPhase;
    public Transform dragStartPosition;
    public Transform tipTransform;
    public Transform gripPoint;

    private Vector3 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        originalPosition = transform.position;
        originalParent = transform.parent;
    }

    public Vector3 GetOriginalPosition() => originalPosition;
    public Transform GetOriginalParent() => originalParent;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.OnItemClicked(this);
    }
}