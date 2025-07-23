// ClickableItem.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableItem : MonoBehaviour, IPointerClickHandler
{
    public Transform dragStartPosition;

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