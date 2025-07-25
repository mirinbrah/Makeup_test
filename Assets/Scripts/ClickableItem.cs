using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки Предмета")]
    public GamePhase itemPhase;
    public Transform dragStartPosition;

    public Vector3 originalPosition;

    void Awake()
    {
        originalPosition = transform.position;
    }

    public Vector3 GetOriginalPosition()
    {
        return originalPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dragStartPosition == null)
        {
            Debug.LogError($"У объекта {gameObject.name} не назначена точка 'Drag Start Position'!", this);
            return;
        }
        GameManager.Instance.OnItemClicked(this);
    }
}