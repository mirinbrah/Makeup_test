using UnityEngine;
using System.Collections;

public class SDKSimulator : MonoBehaviour
{
    public GameObject adPanel;

    public float delayInSeconds = 5f;

    void Start()
    {
        if (adPanel != null)
        {
            adPanel.SetActive(false);
        }

        StartCoroutine(ActivateObjectAfterDelay());
    }

    private IEnumerator ActivateObjectAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);
        adPanel.SetActive(true);
        Debug.Log("SDKSimulator: Объект '" + adPanel.name + "' был активирован после " + delayInSeconds + " секунд.");
    }
}