using UnityEngine;
public class BrushTool : MonoBehaviour
{
    [Tooltip("Точка, за которую 'держится' рука")]
    public Transform gripPoint;

    [Tooltip("SpriteRenderer кончика кисточки для окрашивания")]
    public SpriteRenderer tipSpriteRenderer;

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
        tipSpriteRenderer.color = newColor;
    }
}