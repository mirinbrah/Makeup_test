using UnityEngine;
using UnityEngine.EventSystems;

public class ColorSource : MonoBehaviour, IPointerClickHandler
{
    public GamePhase itemPhase;
    public Color itemColor = Color.white;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.OnMakeupColorSelected(this);
    }
}