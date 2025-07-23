// GameManager.cs (Версия для простого HandController)
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState { Idle, AnimatingToItem, AnimatingToDragPos, PlayerControl, AnimatingToFace, Applying, ReturningSequence }
    private GameState currentState;

    [Header("Объекты сцены")]
    public HandController hand;
    public GameObject acneSprite;
    public Collider2D faceZone;

    [Header("Маркеры позиций")]
    public Transform handStartPosition;
    public Transform handApplyPosition;

    [Header("Настройки")]
    public float moveSpeed = 5f;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    private bool isBusy = false;
    private ClickableItem currentItem;
    private Vector3 itemReturnPosition;

    void Awake() { Instance = this; }

    void Start()
    {
        currentState = GameState.Idle;
        hand.transform.position = handStartPosition.position;
        if (acneSprite != null) acneSprite.SetActive(true);
    }

    public void OnItemClicked(ClickableItem item)
    {
        if (isBusy) return;
        isBusy = true;
        currentItem = item;
        itemReturnPosition = item.transform.position;
        currentState = GameState.AnimatingToItem;
        hand.MoveTo(item.transform.position, moveSpeed, OnMovementComplete);
    }

    public void OnDragEnded(Vector3 dropPosition)
    {
        if (currentState != GameState.PlayerControl) return;
        if (faceZone.OverlapPoint(dropPosition))
        {
            hand.isDraggable = false;
            currentState = GameState.AnimatingToFace;
            hand.MoveTo(handApplyPosition.position, moveSpeed, OnMovementComplete);
        }
        else
        {
            hand.isDraggable = false;
            hand.MoveTo(currentItem.dragStartPosition.position, moveSpeed, () => {
                hand.isDraggable = true;
            });
        }
    }

    private void OnMovementComplete()
    {
        switch (currentState)
        {
            case GameState.AnimatingToItem:
                hand.AttachItem(currentItem.transform);
                currentState = GameState.AnimatingToDragPos;
                hand.MoveTo(currentItem.dragStartPosition.position, moveSpeed, OnMovementComplete);
                break;
            case GameState.AnimatingToDragPos:
                currentState = GameState.PlayerControl;
                hand.isDraggable = true;
                break;
            case GameState.AnimatingToFace:
                StartCoroutine(ApplyAndReturnSequence());
                break;
        }
    }

    private IEnumerator ApplyAndReturnSequence()
    {
        currentState = GameState.Applying;
        yield return StartCoroutine(ShakeHand());
        if (acneSprite != null) acneSprite.SetActive(false);

        hand.DetachItem(currentItem.transform);
        bool finishedMove = false;
        hand.MoveTo(itemReturnPosition, moveSpeed, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);
        currentItem.transform.position = itemReturnPosition;

        finishedMove = false;
        hand.MoveTo(handStartPosition.position, moveSpeed, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        currentState = GameState.Idle;
        isBusy = false;
    }

    private IEnumerator ShakeHand()
    {
        Vector3 originalPosition = hand.transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * shakeMagnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * shakeMagnitude;
            hand.transform.position = new Vector3(x, y, originalPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        hand.transform.position = originalPosition;
    }
}