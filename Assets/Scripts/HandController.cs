// HandController.cs (Финальная версия)
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class HandController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Настройки Руки")]
    public Transform gripPoint;
    public float moveSpeed = 5f;
    public Transform startPositionMarker; 

    private ClickableItem attachedItem;
    private Vector3 autoTargetPosition;
    private Action onMovementComplete;
    private bool isMovingAutomated = false;
    [HideInInspector] public bool isDraggable = false;
    private Vector3 dragOffset;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (gripPoint == null) gripPoint = this.transform;
        if (startPositionMarker == null)
        {
            Debug.LogError("У HandController не назначен маркер стартовой позиции!", this);
        }
    }

    void Start()
    {
        // При старте игры рука сразу перемещается на свою стартовую позицию
        if (startPositionMarker != null)
        {
            transform.position = startPositionMarker.position;
        }
    }

    public void AttachItem(ClickableItem item)
    {
        attachedItem = item;
        attachedItem.transform.SetParent(gripPoint);
        attachedItem.transform.localPosition = Vector3.zero;
    }

    public void DetachItem()
    {
        if (attachedItem == null) return;
        attachedItem.transform.SetParent(null);
        attachedItem = null;
    }

    public ClickableItem GetAttachedItem()
    {
        return attachedItem;
    }

    public void MoveTo(Vector3 target, Action onCompleteCallback)
    {
        autoTargetPosition = target;
        onMovementComplete = onCompleteCallback;
        isMovingAutomated = true;
    }

    // Новый метод для возврата на стартовую позицию
    public void ReturnToStartPosition(Action onCompleteCallback)
    {
        if (startPositionMarker != null)
        {
            MoveTo(startPositionMarker.position, onCompleteCallback);
        }
        else
        {
            onCompleteCallback?.Invoke(); // Сразу завершаем, если нет маркера
        }
    }

    void Update()
    {
        if (!isMovingAutomated) return;
        if (Vector3.Distance(transform.position, autoTargetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, autoTargetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = autoTargetPosition;
            isMovingAutomated = false;
            onMovementComplete?.Invoke();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || isMovingAutomated) return;
        dragOffset = transform.position - GetMouseWorldPos();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || isMovingAutomated) return;
        transform.position = GetMouseWorldPos() + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable || isMovingAutomated) return;
        GameManager.Instance.OnDragEnded();
    }

    private Vector3 GetMouseWorldPos()
    {
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}