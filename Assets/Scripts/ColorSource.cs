using UnityEngine;
using UnityEngine.EventSystems;

public class ColorSource : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Фаза игры, к которой относится этот цвет")]
    public GamePhase itemPhase;

    [Tooltip("Цвет, который представляет этот источник")]
    public Color itemColor = Color.white;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.OnBlushColorSelected(this);
    }
}