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

    public void AnimateBlushPickup(Vector3 palettePosition, Action onComplete)
    {
        onSequenceComplete = onComplete;
        animationTargetPosition = palettePosition;
        StartCoroutine(RotateCoroutine(blushRotationAngle, blushRotationDuration, () => {
            MoveToPalette(() => {
                StartCoroutine(DabCoroutine(() => {
                    StartCoroutine(RotateCoroutine(0, blushRotationDuration, onSequenceComplete));
                }));
            });
        }));
    }

    // --- ИЗМЕНЕННЫЙ МЕТОД ---
    public void AnimateBlushApplication(Vector3 facePosition, Action onApplicationComplete, Action onSequenceFinished)
    {
        animationTargetPosition = facePosition;
        MoveToFaceTarget(() => {
            PerformFaceDabAnimation(onApplicationComplete, onSequenceFinished);
        });
    }

    private void MoveToPalette(Action onComplete)
    {
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null) { onComplete?.Invoke(); return; }
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;
        Vector3 finalHandPosition = animationTargetPosition - handToTipOffset;
        handController.MoveTo(finalHandPosition, onComplete);
    }

    private void MoveToFaceTarget(Action onComplete)
    {
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null) { onComplete?.Invoke(); return; }
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;
        Vector3 finalHandPosition = animationTargetPosition - handToTipOffset;
        handController.MoveTo(finalHandPosition, onComplete);
    }

    // --- ИЗМЕНЕННЫЙ МЕТОД ---
    private void PerformFaceDabAnimation(Action onApplicationComplete, Action onSequenceFinished)
    {
        StartCoroutine(DabCoroutine(() => {
            // Сразу после "возюканья" на лице:
            // 1. Вызываем первый колбэк (чтобы GameManager включил румяна)
            onApplicationComplete?.Invoke();

            // 2. Сбрасываем цвет кисточки
            BrushTool currentTool = handController.GetAttachedTool();
            if (currentTool != null)
            {
                currentTool.SetTipColor(Color.white);
            }

            // 3. Начинаем возврат кисточки, передавая ему ВТОРОЙ колбэк
            ReturnBrushToOriginalPosition(onSequenceFinished);
        }));
    }

    // --- ИЗМЕНЕННЫЙ МЕТОД ---
    private void ReturnBrushToOriginalPosition(Action onSequenceFinished)
    {
        onSequenceComplete = onSequenceFinished; // Сохраняем финальный колбэк
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null) { onSequenceComplete?.Invoke(); return; }
        handController.MoveTo(currentTool.GetOriginalPosition(), DetachBrushAndRetreatHand);
    }

    private void DetachBrushAndRetreatHand()
    {
        handController.DetachAll();
        handController.ReturnToStartPosition(onSequenceComplete);
    }

    public void AnimateCreamApplication(Action onComplete)
    {
        onSequenceComplete = onComplete;
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null || applyAnimationPositionMarker == null) { onSequenceComplete?.Invoke(); return; }
        handController.MoveTo(applyAnimationPositionMarker.position, PerformCreamApplicationAnimation);
    }

    private void PerformCreamApplicationAnimation()
    {
        StartCoroutine(CreamCircularMovementCoroutine(InitiateCreamItemReturn));
    }

    private void InitiateCreamItemReturn()
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        handController.MoveTo(itemReturnPosition, DetachCreamItemAndRetreatHandForCream);
    }

    private void DetachCreamItemAndRetreatHandForCream()
    {
        handController.DetachAll();
        handController.ReturnToStartPosition(onSequenceComplete);
    }

    public void AnimateItemReturn(Action onComplete)
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null) { onComplete?.Invoke(); return; }
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        handController.MoveTo(itemReturnPosition, () => {
            handController.DetachAll();
            handController.ReturnToStartPosition(onComplete);
        });
    }

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

    private IEnumerator DabCoroutine(Action onComplete)
    {
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null) { onComplete?.Invoke(); yield break; }
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;
        Vector3 handCenterPoint = animationTargetPosition - handToTipOffset;
        float elapsedTime = 0f;
        while (elapsedTime < blushDabDuration)
        {
            elapsedTime += Time.deltaTime;
            float xOffset = Mathf.Sin(Time.time * blushDabSpeed) * blushDabDistance;
            transform.position = handCenterPoint + new Vector3(xOffset, 0, 0);
            yield return null;
        }
        transform.position = handCenterPoint;
        onComplete?.Invoke();
    }

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
}