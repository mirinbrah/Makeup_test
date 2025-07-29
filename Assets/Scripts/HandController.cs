using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class HandController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Настройки Руки")]
    public Transform gripPoint;
    public float moveSpeed = 5f;
    public Transform startPositionMarker;

    [Header("Настройки Сортировки")]
    public int activeSortingOrder = 100;

    private ClickableItem attachedItem;
    private BrushTool attachedTool;
    private Vector3 autoTargetPosition;
    private Action onMovementComplete;
    private bool isMovingAutomated = false;
    [HideInInspector] public bool isDraggable = false;
    private Vector3 dragOffset;
    private Camera mainCamera;

    private Dictionary<Renderer, int> originalOrders;

    void Awake()
    {
        mainCamera = Camera.main;
        if (gripPoint == null) gripPoint = this.transform;

        originalOrders = new Dictionary<Renderer, int>();
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

        SaveOriginalOrders(item.transform);
        SetOrderActive(item.transform, true);

        item.transform.SetParent(transform);
        item.transform.rotation = transform.rotation;
        Vector3 itemOffset = item.transform.position - item.gripPoint.position;
        item.transform.position = this.gripPoint.position + itemOffset;
    }

    public void AttachTool(BrushTool tool)
    {
        DetachAll();
        attachedTool = tool;

        SaveOriginalOrders(tool.transform);
        SetOrderActive(tool.transform, true);

        tool.transform.SetParent(transform);
        tool.transform.rotation = transform.rotation;
        Vector3 toolOffset = tool.transform.position - tool.gripPoint.position;
        tool.transform.position = this.gripPoint.position + toolOffset;
    }

    public void DetachAll()
    {
        RestoreOriginalOrders();

        if (attachedItem != null)
        {
            attachedItem.transform.SetParent(attachedItem.GetOriginalParent());
            attachedItem.transform.position = attachedItem.GetOriginalPosition();
            attachedItem = null;
        }
        if (attachedTool != null)
        {
            attachedTool.transform.SetParent(attachedTool.GetOriginalParent());
            attachedTool.transform.position = attachedTool.GetOriginalPosition();
            attachedTool.transform.rotation = attachedTool.GetOriginalRotation();
            attachedTool = null;
        }
    }

    public void SetOrderActive(bool isActive)
    {
        if (isActive)
        {
            SaveOriginalOrders(transform);
            SetOrderActive(transform, true);
        }
        else
        {
            RestoreOriginalOrders();
        }
    }

    private void SaveOriginalOrders(Transform parent)
    {
        if (parent == null) return;
        originalOrders.Clear();

        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            originalOrders[renderer] = renderer.sortingOrder;
        }
    }

    private void RestoreOriginalOrders()
    {
        if (originalOrders == null) return;
        foreach (var pair in originalOrders)
        {
            if (pair.Key != null)
            {
                pair.Key.sortingOrder = pair.Value;
            }
        }
        originalOrders.Clear();
    }

    private void SetOrderActive(Transform parent, bool isActive)
    {
        if (parent == null) return;
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (isActive)
            {
                renderer.sortingOrder = activeSortingOrder + (originalOrders.ContainsKey(renderer) ? originalOrders[renderer] : 0);
            }
        }
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
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
}