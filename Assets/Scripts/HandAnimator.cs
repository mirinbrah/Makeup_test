using UnityEngine;
using System;
using System.Collections;

public class HandAnimator : MonoBehaviour
{
    [Header("Настройки Аниматора")]
    public Transform applyAnimationPositionMarker;

    [Header("Настройки Анимации 'Нанесение'")]
    public float applyDuration = 1.5f;
    public float applyRadius = 0.5f;

    private HandController handController;
    private Action onAnimationComplete;

    void Awake()
    {
        handController = GetComponent<HandController>();
    }

    // Главный метод, который запускает всю цепочку анимации
    public void AnimateApplyAndReturn(Action onComplete)
    {
        onAnimationComplete = onComplete;

        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null || applyAnimationPositionMarker == null)
        {
            Debug.LogError("HandAnimator не может начать анимацию: нет предмета или маркера позиции!");
            onAnimationComplete?.Invoke();
            return;
        }

        // Шаг 1: Двигаемся к точке анимации. По завершении вызываем Step2_CircularMovement.
        handController.MoveTo(applyAnimationPositionMarker.position, Step2_CircularMovement);
    }

    // Шаг 2: Выполняем круговое движение.
    private void Step2_CircularMovement()
    {
        StartCoroutine(CircularMovementCoroutine(Step3_ReturnItem));
    }

    // Корутина для самой анимации вращения. По завершении вызывает следующий шаг.
    private IEnumerator CircularMovementCoroutine(Action onComplete)
    {
        Vector3 centerPoint = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < applyDuration)
        {
            elapsedTime += Time.deltaTime;
            float angle = (elapsedTime / applyDuration) * 2 * Mathf.PI;
            float xOffset = Mathf.Cos(angle) * applyRadius;
            float yOffset = Mathf.Sin(angle) * applyRadius;
            transform.position = centerPoint + new Vector3(xOffset, yOffset, 0);
            yield return null;
        }
        transform.position = centerPoint;
        onComplete?.Invoke();
    }

    // Шаг 3: Возвращаем предмет на место.
    private void Step3_ReturnItem()
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();

        // Двигаем руку с предметом на его исходное место. По завершении вызываем Step4_DetachAndLeave.
        handController.MoveTo(itemReturnPosition, () => Step4_DetachAndLeave(currentItem, itemReturnPosition));
    }

    // Шаг 4: Отсоединяем предмет и уводим руку.
    private void Step4_DetachAndLeave(ClickableItem item, Vector3 position)
    {
        handController.DetachItem();
        item.transform.position = position;

        // Возвращаем пустую руку на старт. По завершении вызываем финальный колбэк.
        handController.ReturnToStartPosition(onAnimationComplete);
    }

    // Метод для простого возврата (для кнопки Reset)
    public void AnimateReturnOnly(Action onComplete)
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null)
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();

        // Двигаем руку с предметом
        handController.MoveTo(itemReturnPosition, () => {
            // Отсоединяем
            handController.DetachItem();
            currentItem.transform.position = itemReturnPosition;
            // Уводим пустую руку
            handController.ReturnToStartPosition(onComplete);
        });
    }
}