using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HandController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Настройки Руки")]
    public Transform gripPoint;
    public float moveSpeed = 5f;
    public Transform startPositionMarker;

    [Header("Визуал Руки для Сортировки")]
    public SpriteRenderer handTopRenderer;
    public SpriteRenderer handBottomRenderer;

    [Header("Настройки Сортировки")]
    public int idleSortingOrder = 100;   
    public int activeSortingOrder = 200;  

    private ClickableItem attachedItem;
    private BrushTool attachedTool;
    private Vector3 autoTargetPosition;
    private Action onMovementComplete;
    private bool isMovingAutomated = false;
    [HideInInspector] public bool isDraggable = false;
    private Vector3 dragOffset;
    private Camera mainCamera;

    private Dictionary<Renderer, int> attachedObjectOriginalOrders = new Dictionary<Renderer, int>();

    void Awake()
    {
        mainCamera = Camera.main;
        if (gripPoint == null) gripPoint = this.transform;
    }

    void Start()
    {
        if (startPositionMarker != null)
        {
            transform.position = startPositionMarker.position;
        }
        SetIdleSorting();
    }

    private void SetIdleSorting()
    {
        if (handBottomRenderer != null) handBottomRenderer.sortingOrder = idleSortingOrder;
        if (handTopRenderer != null) handTopRenderer.sortingOrder = idleSortingOrder + 1;
    }

    private void SetActiveSorting()
    {
        if (handBottomRenderer != null) handBottomRenderer.sortingOrder = activeSortingOrder;
        if (handTopRenderer != null) handTopRenderer.sortingOrder = activeSortingOrder + 2;
    }

    public void AttachItem(ClickableItem item)
    {
        DetachAll();
        attachedItem = item;

        SetActiveSorting();

        attachedObjectOriginalOrders.Clear();
        foreach (var rend in item.GetComponentsInChildren<Renderer>(true))
        {
            attachedObjectOriginalOrders[rend] = rend.sortingOrder;
            rend.sortingOrder = activeSortingOrder + 1; 
        }

        item.transform.SetParent(transform);
        item.transform.rotation = transform.rotation;
        Vector3 itemOffset = item.transform.position - item.gripPoint.position;
        item.transform.position = this.gripPoint.position + itemOffset;
    }

    public void AttachTool(BrushTool tool)
    {
        DetachAll();
        attachedTool = tool;

        SetActiveSorting();

        attachedObjectOriginalOrders.Clear();
        foreach (var rend in tool.GetComponentsInChildren<Renderer>(true))
        {
            attachedObjectOriginalOrders[rend] = rend.sortingOrder;
            rend.sortingOrder = activeSortingOrder + 1; 
        }

        tool.transform.SetParent(transform);
        tool.transform.rotation = transform.rotation;
        Vector3 toolOffset = tool.transform.position - tool.gripPoint.position;
        tool.transform.position = this.gripPoint.position + toolOffset;
    }

    public void DetachAll()
    {
        SetIdleSorting();

        if (attachedItem != null)
        {
            foreach (var pair in attachedObjectOriginalOrders)
            {
                if (pair.Key != null) pair.Key.sortingOrder = pair.Value;
            }
            attachedItem.transform.SetParent(attachedItem.GetOriginalParent());
            attachedItem.transform.position = attachedItem.GetOriginalPosition();
            attachedItem = null;
        }
        if (attachedTool != null)
        {
            foreach (var pair in attachedObjectOriginalOrders)
            {
                if (pair.Key != null) pair.Key.sortingOrder = pair.Value;
            }
            attachedTool.transform.SetParent(attachedTool.GetOriginalParent());
            attachedTool.transform.position = attachedTool.GetOriginalPosition();
            attachedTool.transform.rotation = attachedTool.GetOriginalRotation();
            attachedTool = null;
        }
        attachedObjectOriginalOrders.Clear();
    }

    public ClickableItem GetAttachedItem()
    {
        return attachedItem;
    }

    public BrushTool GetAttachedTool()
    {
        return attachedTool;
    }

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
        if (Pointer.current == null)
        {
            return transform.position;
        }
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }
}