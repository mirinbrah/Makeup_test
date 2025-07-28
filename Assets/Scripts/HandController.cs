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
    private BrushTool attachedTool; // Убедитесь, что это поле у вас есть
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
        if (startPositionMarker != null)
        {
            transform.position = startPositionMarker.position;
        }
    }

    public void AttachItem(ClickableItem item)
    {
        DetachAll();
        attachedItem = item;
        attachedItem.transform.SetParent(gripPoint);
        attachedItem.transform.localPosition = Vector3.zero;
    }

    public void AttachTool(BrushTool tool)
    {
        DetachAll();
        attachedTool = tool;
        tool.transform.SetParent(transform);
        tool.transform.rotation = transform.rotation;
        Vector3 toolOffset = tool.transform.position - tool.gripPoint.position;
        tool.transform.position = this.gripPoint.position + toolOffset;
    }

    public void DetachAll()
    {
        if (attachedItem != null)
        {
            attachedItem.transform.SetParent(null);
            attachedItem.transform.position = attachedItem.GetOriginalPosition();
            attachedItem = null;
        }
        if (attachedTool != null)
        {
            attachedTool.transform.SetParent(null);
            attachedTool.transform.position = attachedTool.GetOriginalPosition();
            attachedTool.transform.rotation = attachedTool.GetOriginalRotation();
            attachedTool = null;
        }
    }

    public ClickableItem GetAttachedItem()
    {
        return attachedItem;
    }

    // +++ ВОТ НЕДОСТАЮЩИЙ МЕТОД +++
    /// <summary>
    /// Возвращает прикрепленный к руке инструмент (кисточку).
    /// </summary>
    /// <returns>Компонент BrushTool или null, если ничего не прикреплено.</returns>
    public BrushTool GetAttachedTool()
    {
        return attachedTool;
    }
    // +++++++++++++++++++++++++++++++

    public void MoveTo(Vector3 target, Action onCompleteCallback)
    {
        autoTargetPosition = target;
        onMovementComplete = onCompleteCallback;
        isMovingAutomated = true;
    }

    public void ReturnToStartPosition(Action onCompleteCallback)
    {
        if (startPositionMarker != null)
        {
            MoveTo(startPositionMarker.position, onCompleteCallback);
        }
        else
        {
            onCompleteCallback?.Invoke();
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
        // Для 2D игры лучше обнулять Z координату
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
}