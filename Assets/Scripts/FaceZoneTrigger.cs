using UnityEngine;

public class FaceZoneTrigger : MonoBehaviour
{
    private const string TOOL_TAG = "Tool";
    private const string CREAM_TAG = "Cream";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(TOOL_TAG) || other.CompareTag(CREAM_TAG))
        {
            GameManager.Instance.OnItemReachedTargetZone();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(TOOL_TAG) || other.CompareTag(CREAM_TAG))
        {
            GameManager.Instance.OnItemLeftTargetZone();
        }
    }
}