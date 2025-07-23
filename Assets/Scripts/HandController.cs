// HandController.cs (Простая версия с отключением коллайдера)
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class HandController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Настройки Руки")]
    public Transform gripPoint;

    private Collider2D attachedItemCollider; // Храним ссылку на коллайдер предмета

    // --- Переменные для движения и перетаскивания ---
    private Vector3 autoTargetPosition;
    private float moveSpeed;
    private Action onMovementComplete;
    private bool isMovingAutomated = false;
    public bool isDraggable = false;
    private Vector3 dragOffset;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (gripPoint == null) gripPoint = this.transform;
    }

    // --- Методы для управления предметом ---
    public void AttachItem(Transform item)
    {
        item.SetParent(gripPoint);
        item.localPosition = Vector3.zero;

        // Находим и ОТКЛЮЧАЕМ коллайдер
        attachedItemCollider = item.GetComponent<Collider2D>();
        if (attachedItemCollider != null)
        {
            attachedItemCollider.enabled = false;
        }
    }

    public void DetachItem(Transform item)
    {
        // ВКЛЮЧАЕМ коллайдер обратно
        if (attachedItemCollider != null)
        {
            attachedItemCollider.enabled = true;
        }
        item.SetParent(null);
        attachedItemCollider = null;
    }

    // --- Остальная логика без изменений ---
    public void MoveTo(Vector3 target, float speed, Action onCompleteCallback)
    {
        autoTargetPosition = target;
        moveSpeed = speed;
        onMovementComplete = onCompleteCallback;
        isMovingAutomated = true;
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
        GameManager.Instance.OnDragEnded(transform.position);
    }

    private Vector3 GetMouseWorldPos()
    {
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}