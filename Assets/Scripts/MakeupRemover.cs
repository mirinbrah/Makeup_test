// MakeupRemover.cs
using UnityEngine;
using UnityEngine.EventSystems;

// Этот скрипт вешается на объект спонжика
public class MakeupRemover : MonoBehaviour, IPointerClickHandler
{
    // Этот метод вызывается, когда на объект кликают
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Спонжик нажат. Вызываю сброс макияжа.");

        // Просто сообщаем GameManager, что нужно все стереть
        GameManager.Instance.RemoveAllMakeup();
    }
}