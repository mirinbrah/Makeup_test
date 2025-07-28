using UnityEngine;
using System;
using System.Collections;

public class HandAnimator : MonoBehaviour
{
    [Header("Общие Настройки")]
    public Transform applyAnimationPositionMarker;

    [Header("Настройки Анимации 'Крем'")]
    public float creamApplyDuration = 1.5f;
    public float creamApplyRadius = 0.5f;

    [Header("Настройки Анимации 'Румяна'")]
    public float blushRotationAngle = 45f;
    public float blushRotationDuration = 0.3f;
    public float blushDabDuration = 1.0f;
    public float blushDabDistance = 0.1f;
    public float blushDabSpeed = 15f;

    private HandController handController;
    private Action onSequenceComplete;
    private Vector3 animationTargetPosition;

    void Awake()
    {
        handController = GetComponent<HandController>();
    }

    #region Анимации для Румян

    /// <summary>
    /// Запускает полную последовательность анимации для взятия румян из палетки.
    /// </summary>
    public void AnimateBlushPickup(Vector3 palettePosition, Action onComplete)
    {
        onSequenceComplete = onComplete;
        animationTargetPosition = palettePosition;

        // Начинаем с поворота руки, по завершении которого вызывается следующий метод в цепочке.
        StartCoroutine(RotateCoroutine(blushRotationAngle, blushRotationDuration, MoveToPalette));
    }

    // Вызывается после завершения начального поворота.
    private void MoveToPalette()
    {
        // --- НАЧАЛО ИЗМЕНЕНИЙ ---

        // 1. Получаем текущий инструмент, чтобы найти его кончик
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null)
        {
            Debug.LogError("Невозможно рассчитать позицию для движения: не найден инструмент или его кончик (tipTransform)!");
            onSequenceComplete?.Invoke(); // Безопасно завершаем анимацию
            return;
        }

        // 2. Вычисляем вектор смещения от ЦЕНТРА РУКИ до КОНЧИКА КИСТОЧКИ.
        // Этот вектор показывает, "где находится кончик относительно руки".
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;

        // 3. Вычисляем ФИНАЛЬНУЮ ПОЗИЦИЮ для РУКИ.
        // Цель: animationTargetPosition (центр цвета)
        // Мы хотим, чтобы КОНЧИК оказался на цели. Значит, РУКА должна быть в точке (цель - смещение).
        Vector3 finalHandPosition = animationTargetPosition - handToTipOffset;

        // 4. Говорим руке плавно двигаться в эту новую, правильно рассчитанную точку.
        handController.MoveTo(finalHandPosition, PerformDabAnimation);

        // --- КОНЕЦ ИЗМЕНЕНИЙ ---
    }

    // Вызывается после прибытия к палетке.
    private void PerformDabAnimation()
    {
        StartCoroutine(DabCoroutine(FinalizeBlushPickup));
    }

    // Вызывается после завершения "возюканья".
    private void FinalizeBlushPickup()
    {
        // Возвращаем руку в исходный поворот и по завершении вызываем финальный колбэк.
        StartCoroutine(RotateCoroutine(0, blushRotationDuration, onSequenceComplete));
    }

    #endregion

    #region Анимации для Крема

    /// <summary>
    /// Запускает полную последовательность анимации нанесения крема.
    /// </summary>
    public void AnimateCreamApplication(Action onComplete)
    {
        onSequenceComplete = onComplete;

        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null || applyAnimationPositionMarker == null)
        {
            Debug.LogError("HandAnimator не может начать анимацию крема: нет предмета или маркера позиции!");
            onSequenceComplete?.Invoke();
            return;
        }
        handController.MoveTo(applyAnimationPositionMarker.position, PerformCreamApplicationAnimation);
    }

    // Вызывается после прибытия к точке нанесения крема.
    private void PerformCreamApplicationAnimation()
    {
        StartCoroutine(CreamCircularMovementCoroutine(InitiateCreamItemReturn));
    }

    // Вызывается после завершения круговой анимации.
    private void InitiateCreamItemReturn()
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        handController.MoveTo(itemReturnPosition, DetachCreamItemAndRetreatHand);
    }

    // Вызывается после возвращения предмета на его место.
    private void DetachCreamItemAndRetreatHand()
    {
        handController.DetachAll();
        handController.ReturnToStartPosition(onSequenceComplete);
    }

    #endregion

    #region Общие Анимации и Корутины

    /// <summary>
    /// Анимирует возврат предмета на его исходное место.
    /// </summary>
    public void AnimateItemReturn(Action onComplete)
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null)
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        handController.MoveTo(itemReturnPosition, () => {
            handController.DetachAll();
            handController.ReturnToStartPosition(onComplete);
        });
    }

    // Корутина для плавного поворота руки.
    private IEnumerator RotateCoroutine(float targetAngle, float duration, Action onComplete)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
        onComplete?.Invoke();
    }

    // Корутина для "возюканья" кисточкой влево-вправо.
    private IEnumerator DabCoroutine(Action onComplete)
    {
        // Получаем текущий инструмент из HandController
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null)
        {
            Debug.LogError("Анимация 'возюканья' невозможна: не найден инструмент или его кончик (tipTransform)!");
            onComplete?.Invoke();
            yield break; // Прерываем корутину, если что-то не так
        }

        // --- КЛЮЧЕВАЯ ЛОГИКА ---
        // 1. Вычисляем вектор смещения от центра руки до кончика кисточки.
        // Этот вектор остается постоянным, пока кисточка в руке.
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;

        // 2. Определяем центральную точку для РУКИ.
        // Она должна быть такой, чтобы КОНЧИК КИСТОЧКИ оказался точно над целью (animationTargetPosition).
        Vector3 handCenterPoint = animationTargetPosition - handToTipOffset;

        float elapsedTime = 0f;
        while (elapsedTime < blushDabDuration)
        {
            elapsedTime += Time.deltaTime;
            // Используем синус для плавного движения туда-сюда
            float xOffset = Mathf.Sin(Time.time * blushDabSpeed) * blushDabDistance;

            // 3. Двигаем РУКУ вокруг ее вычисленной центральной точки.
            // В результате кончик кисточки будет двигаться вокруг цели.
            transform.position = handCenterPoint + new Vector3(xOffset, 0, 0);

            yield return null;
        }

        // 4. По завершении ставим руку в финальную центральную позицию.
        transform.position = handCenterPoint;
        onComplete?.Invoke();
    }

    // Корутина для кругового движения при нанесении крема.
    private IEnumerator CreamCircularMovementCoroutine(Action onComplete)
    {
        Vector3 centerPoint = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < creamApplyDuration)
        {
            elapsedTime += Time.deltaTime;
            float angle = (elapsedTime / creamApplyDuration) * 2 * Mathf.PI;
            float xOffset = Mathf.Cos(angle) * creamApplyRadius;
            float yOffset = Mathf.Sin(angle) * creamApplyRadius;
            transform.position = centerPoint + new Vector3(xOffset, yOffset, 0);
            yield return null;
        }
        transform.position = centerPoint;
        onComplete?.Invoke();
    }

    #endregion
}