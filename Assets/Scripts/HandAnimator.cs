using UnityEngine;
using System.Collections;
using System;

public class HandAnimator : MonoBehaviour
{
    [Header("Настройки Аниматора")]
    public Transform applyAnimationPositionMarker;

    [Header("Настройки Анимации 'Нанесение'")]
    public float applyDuration = 1.5f; // Время на один полный круг
    public float applyRadius = 0.5f;   // Радиус кругового движения

    private HandController handController;

    void Awake()
    {
        handController = GetComponent<HandController>();
    }

    public IEnumerator AnimateApplyAndReturn(Action onComplete)
    {
        if (applyAnimationPositionMarker == null)
        {
            Debug.LogError("У HandAnimator не назначен маркер 'applyAnimationPositionMarker'!", this);
            onComplete?.Invoke();
            yield break;
        }

        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null)
        {
            Debug.LogError("HandAnimator пытался начать анимацию, но рука ничего не держит.", this);
            onComplete?.Invoke();
            yield break;
        }

        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();

        // 1. Движение к точке анимации (центру будущего круга)
        bool finishedMove = false;
        handController.MoveTo(applyAnimationPositionMarker.position, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        // 2. АНИМАЦИЯ КРУГОВОГО ДВИЖЕНИЯ
        Vector3 centerPoint = transform.position; // Запоминаем центр круга
        float elapsedTime = 0f;

        while (elapsedTime < applyDuration)
        {
            elapsedTime += Time.deltaTime;
            float angle = (elapsedTime / applyDuration) * 2 * Mathf.PI; // Угол в радианах (от 0 до 2*PI)

            // Вычисляем смещение по X и Y с помощью тригонометрии
            float xOffset = Mathf.Cos(angle) * applyRadius;
            float yOffset = Mathf.Sin(angle) * applyRadius;

            // Применяем смещение к центральной точке
            transform.position = centerPoint + new Vector3(xOffset, yOffset, 0);

            yield return null;
        }
        transform.position = centerPoint; // Возвращаем руку точно в центр

        // 3. Рука с предметом едет на исходное место предмета
        finishedMove = false;
        handController.MoveTo(itemReturnPosition, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        // 4. Рука отпускает предмет
        handController.DetachItem();
        currentItem.transform.position = itemReturnPosition;

        // 5. Пустая рука уезжает
        finishedMove = false;
        handController.ReturnToStartPosition(() => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        onComplete?.Invoke();
    }

    public IEnumerator AnimateReturnOnly(Action onComplete)
    {
        // Этот метод остается без изменений
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null) { onComplete?.Invoke(); yield break; }

        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();

        bool finishedMove = false;
        handController.MoveTo(itemReturnPosition, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        handController.DetachItem();
        currentItem.transform.position = itemReturnPosition;

        finishedMove = false;
        handController.ReturnToStartPosition(() => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        onComplete?.Invoke();
    }
}