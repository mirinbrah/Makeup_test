using UnityEngine;

public class TabsSwitcher : MonoBehaviour
{
    public GamePhase phase;
    public GameObject activeTab;
    public GameObject unactiveTab;
    public GameObject contentContainer;

    void Start()
    {
        UpdateVisual(false);
    }

    public void UpdateVisual(bool isActive)
    {
        if (activeTab != null) activeTab.SetActive(isActive);
        if (unactiveTab != null) unactiveTab.SetActive(!isActive);
        if (contentContainer != null) contentContainer.SetActive(isActive);
    }

    private void OnMouseDown()
    {
        GameManager.Instance.SwitchToPhase(phase);
    }
}