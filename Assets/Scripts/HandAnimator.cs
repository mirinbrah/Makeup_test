using UnityEngine;
using System;
using System.Collections;

public class HandAnimator : MonoBehaviour
{
    [Header("Маркеры Позиций Анимаций")]
    public Transform creamApplyPositionMarker;
    public Transform lipstickApplyPositionMarker;
    public Transform blushApplyPositionMarker;
    public Transform eyeshadowApplyPositionMarker;

    [Header("Настройки Анимации 'Крем' (Круговая)")]
    public float creamApplyDuration = 1.5f;
    public float creamApplyRadius = 0.5f;

    [Header("Настройки Анимации 'Помада' (Возюканье)")]
    public float lipstickDabDuration = 1.0f;
    public float lipstickDabDistance = 0.1f;
    public float lipstickDabSpeed = 15f;

    [Header("Настройки Анимации 'Макияж с кистью' (Возюканье)")]
    public float makeupRotationAngle = 45f;
    public float makeupRotationDuration = 0.3f;
    public float makeupDabDuration = 1.0f;
    public float makeupDabDistance = 0.1f;
    public float makeupDabSpeed = 15f;

    private HandController handController;
    private Action onSequenceComplete;
    private Vector3 animationTargetPosition;

    void Awake()
    {
        handController = GetComponent<HandController>();
    }

    public void AnimateItemApplication(GamePhase phase, Action onApplicationComplete, Action onSequenceFinished)
    {
        Transform targetMarker = null;
        switch (phase)
        {
            case GamePhase.Acne:
                targetMarker = creamApplyPositionMarker;
                break;
            case GamePhase.Lipstick:
                targetMarker = lipstickApplyPositionMarker;
                break;
        }

        if (targetMarker == null)
        {
            onSequenceFinished?.Invoke();
            return;
        }

        animationTargetPosition = targetMarker.position;
        MoveItemToFaceTarget(() => {
            if (phase == GamePhase.Lipstick)
            {
                StartCoroutine(LipstickDabCoroutine(onApplicationComplete, onSequenceFinished));
            }
            else
            {
                StartCoroutine(CreamCircularMovementCoroutine(onApplicationComplete, onSequenceFinished));
            }
        });
    }

    private void MoveItemToFaceTarget(Action onAnimationStart)
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null || currentItem.tipTransform == null) { onAnimationStart?.Invoke(); return; }

        Vector3 handToTipOffset = currentItem.tipTransform.position - handController.transform.position;
        Vector3 finalHandPosition = animationTargetPosition - handToTipOffset;
        handController.MoveTo(finalHandPosition, onAnimationStart);
    }

    private IEnumerator LipstickDabCoroutine(Action onApplicationComplete, Action onSequenceFinished)
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null || currentItem.tipTransform == null) { onSequenceFinished?.Invoke(); yield break; }

        Vector3 handToTipOffset = currentItem.tipTransform.position - handController.transform.position;
        Vector3 handCenterPoint = animationTargetPosition - handToTipOffset;
        float elapsedTime = 0f;
        while (elapsedTime < lipstickDabDuration)
        {
            elapsedTime += Time.deltaTime;
            float xOffset = Mathf.Sin(Time.time * lipstickDabSpeed) * lipstickDabDistance;
            handController.transform.position = handCenterPoint + new Vector3(xOffset, 0, 0);
            yield return null;
        }
        handController.transform.position = handCenterPoint;

        onApplicationComplete?.Invoke();

        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        handController.MoveTo(itemReturnPosition, () => {
            handController.DetachAll();
            handController.ReturnToStartPosition(onSequenceFinished);
        });
    }

    private IEnumerator CreamCircularMovementCoroutine(Action onApplicationComplete, Action onSequenceFinished)
    {
        Vector3 centerPoint = handController.transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < creamApplyDuration)
        {
            elapsedTime += Time.deltaTime;
            float angle = (elapsedTime / creamApplyDuration) * 2 * Mathf.PI;
            float xOffset = Mathf.Cos(angle) * creamApplyRadius;
            float yOffset = Mathf.Sin(angle) * creamApplyRadius;
            handController.transform.position = centerPoint + new Vector3(xOffset, yOffset, 0);
            yield return null;
        }
        handController.transform.position = centerPoint;

        onApplicationComplete?.Invoke();

        ClickableItem currentItem = handController.GetAttachedItem();
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        handController.MoveTo(itemReturnPosition, () => {
            handController.DetachAll();
            handController.ReturnToStartPosition(onSequenceFinished);
        });
    }

    public void AnimateMakeupPickup(Vector3 palettePosition, Action onComplete)
    {
        onSequenceComplete = onComplete;
        animationTargetPosition = palettePosition;
        StartCoroutine(RotateCoroutine(makeupRotationAngle, makeupRotationDuration, () => {
            MoveToPalette(() => {
                StartCoroutine(BrushDabCoroutine(makeupDabDuration, () => {
                    StartCoroutine(RotateCoroutine(0, makeupRotationDuration, onSequenceComplete));
                }));
            });
        }));
    }

    public void AnimateMakeupApplication(GamePhase phase, Action onApplicationComplete, Action onSequenceFinished)
    {
        Transform targetMarker = null;
        switch (phase)
        {
            case GamePhase.Blush:
                targetMarker = blushApplyPositionMarker;
                break;
            case GamePhase.Eyeshadow:
                targetMarker = eyeshadowApplyPositionMarker;
                break;
        }

        if (targetMarker == null)
        {
            onSequenceFinished?.Invoke();
            return;
        }

        animationTargetPosition = targetMarker.position;
        MoveToFaceTargetBrush(() => {
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

    private void MoveToFaceTargetBrush(Action onComplete)
    {
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null) { onComplete?.Invoke(); return; }
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;
        Vector3 finalHandPosition = animationTargetPosition - handToTipOffset;
        handController.MoveTo(finalHandPosition, onComplete);
    }

    private void PerformFaceDabAnimation(Action onApplicationComplete, Action onSequenceFinished)
    {
        StartCoroutine(BrushDabCoroutine(makeupDabDuration, () => {
            onApplicationComplete?.Invoke();
            BrushTool currentTool = handController.GetAttachedTool();
            if (currentTool != null)
            {
                currentTool.SetTipColor(Color.white);
            }
            ReturnBrushToOriginalPosition(onSequenceFinished);
        }));
    }

    private void ReturnBrushToOriginalPosition(Action onSequenceFinished)
    {
        onSequenceComplete = onSequenceFinished;
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null) { onSequenceComplete?.Invoke(); return; }
        handController.MoveTo(currentTool.GetOriginalPosition(), DetachBrushAndRetreatHand);
    }

    private void DetachBrushAndRetreatHand()
    {
        handController.DetachAll();
        handController.ReturnToStartPosition(onSequenceComplete);
    }

    public void AnimateItemReturn(Action onItemReturned, Action onSequenceFinished)
    {
        ClickableItem currentItem = handController.GetAttachedItem();
        if (currentItem == null)
        {
            onItemReturned?.Invoke();
            onSequenceFinished?.Invoke();
            return;
        }

        Vector3 handToItemOffset = currentItem.transform.position - handController.transform.position;
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();
        Vector3 handTargetPosition = itemReturnPosition - handToItemOffset;

        handController.MoveTo(handTargetPosition, () => {
            handController.DetachAll();

            onItemReturned?.Invoke();

            handController.ReturnToStartPosition(onSequenceFinished);
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

    private IEnumerator BrushDabCoroutine(float duration, Action onComplete)
    {
        BrushTool currentTool = handController.GetAttachedTool();
        if (currentTool == null || currentTool.tipTransform == null) { onComplete?.Invoke(); yield break; }
        Vector3 handToTipOffset = currentTool.tipTransform.position - transform.position;
        Vector3 handCenterPoint = animationTargetPosition - handToTipOffset;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float xOffset = Mathf.Sin(Time.time * makeupDabSpeed) * makeupDabDistance;
            transform.position = handCenterPoint + new Vector3(xOffset, 0, 0);
            yield return null;
        }
        transform.position = handCenterPoint;
        onComplete?.Invoke();
    }
}