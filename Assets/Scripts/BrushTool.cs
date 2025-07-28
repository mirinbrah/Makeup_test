using UnityEngine;

public class BrushTool : MonoBehaviour
{
    [Header("Настройки Инструмента")]
    public Transform gripPoint;
    public SpriteRenderer tipSpriteRenderer;
    public Transform tipTransform;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent; // <-- НОВОЕ ПОЛЕ

    void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent; // <-- ЗАПОМИНАЕМ РОДИТЕЛЯ
    }

    public Vector3 GetOriginalPosition() => originalPosition;
    public Quaternion GetOriginalRotation() => originalRotation;
    public Transform GetOriginalParent() => originalParent; // <-- НОВЫЙ МЕТОД

    public void SetTipColor(Color newColor)
    {
        if (tipSpriteRenderer != null)
        {
            tipSpriteRenderer.color = newColor;
        }
    }
}