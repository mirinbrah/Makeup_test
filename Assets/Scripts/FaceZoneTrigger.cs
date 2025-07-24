using UnityEngine;

public class FaceZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cream"))
        {
            GameManager.Instance.OnItemReachedTargetZone();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Cream"))
        {
            GameManager.Instance.OnItemLeftTargetZone();
        }
    }
}