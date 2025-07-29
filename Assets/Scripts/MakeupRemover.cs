using UnityEngine;
using UnityEngine.EventSystems;

public class MakeupRemover : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.RemoveAllMakeup();
    }
}