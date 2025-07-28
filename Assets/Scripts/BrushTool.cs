// В файле BrushTool.cs

using UnityEngine;

public class BrushTool : MonoBehaviour
{
    [Header("Настройки Инструмента")]
    public Transform gripPoint;
    public SpriteRenderer tipSpriteRenderer; // Оставляем для окрашивания

    // +++ НОВОЕ ПОЛЕ +++
    [Tooltip("Transform кончика кисточки для точного позиционирования анимации")]
    public Transform tipTransform; // Сюда мы перетащим 'blush brush cone'

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public Vector3 GetOriginalPosition() => originalPosition;
    public Quaternion GetOriginalRotation() => originalRotation;

    public void SetTipColor(Color newColor)
    {
        if (tipSpriteRenderer != null)
        {
            tipSpriteRenderer.color = newColor;
        }
    }
}