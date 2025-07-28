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
    private BrushTool attachedTool;

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
        attachedItem = item;
        attachedItem.transform.SetParent(gripPoint);
        attachedItem.transform.localPosition = Vector3.zero;
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
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    public void AttachTool(BrushTool tool)
    {
        DetachAll(); // Отсоединяем все, что могло быть в руке
        attachedTool = tool;

        // 1. Сначала делаем кисточку дочерним объектом руки, чтобы они двигались вместе.
        // Это важно, чтобы локальные координаты работали правильно.
        tool.transform.SetParent(transform);

        // 2. Выравниваем поворот (часто это лучше делать до выравнивания позиции).
        // Кисточка будет смотреть туда же, куда и рука.
        tool.transform.rotation = transform.rotation;

        // 3. Вычисляем вектор смещения от ЦЕНТРА кисточки до ее ТОЧКИ ХВАТА.
        Vector3 toolOffset = tool.transform.position - tool.gripPoint.position;

        // 4. Устанавливаем позицию кисточки.
        // Логика: "Я хочу, чтобы точка хвата кисточки (tool.gripPoint) оказалась
        // точно там же, где и точка хвата руки (this.gripPoint)".
        // Для этого мы берем целевую позицию (this.gripPoint.position)
        // и "отступаем" от нее на вектор смещения кисточки.
        tool.transform.position = this.gripPoint.position + toolOffset;

        Debug.Log("Инструмент " + tool.name + " прикреплен. Точки хвата руки и инструмента совмещены.");
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
}