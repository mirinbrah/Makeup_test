using UnityEngine;
using System.Collections;
using System;

public class HandAnimator : MonoBehaviour
{
    [Header("Настройки Аниматора")]
    public Transform applyAnimationPositionMarker;

    [Header("Настройки Анимации 'Нанесение'")]
    public float applyDuration = 1.5f;
    public float applyMagnitude = 0.5f;
    public int applyCycles = 2;

    private HandController handController;

    void Awake()
    {
        handController = GetComponent<HandController>();
    }

    public IEnumerator AnimateApplyAndReturn(Action onComplete)
    {
        if (applyAnimationPositionMarker == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        ClickableItem currentItem = handController.GetAttachedItem();
        Vector3 itemReturnPosition = currentItem.GetOriginalPosition();

        // 1. Движение к точке анимации
        bool finishedMove = false;
        handController.MoveTo(applyAnimationPositionMarker.position, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        // 2. Анимация покачивания
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < applyDuration)
        {
            elapsedTime += Time.deltaTime;
            float sinWave = Mathf.Sin(elapsedTime / applyDuration * applyCycles * Mathf.PI * 2);
            float xOffset = sinWave * applyMagnitude;
            transform.position = originalPosition + new Vector3(xOffset, 0, 0);
            yield return null;
        }
        transform.position = originalPosition;

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
}