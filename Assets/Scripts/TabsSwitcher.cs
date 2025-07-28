using System;
using UnityEngine;

public class TabsSwitcher : MonoBehaviour
{
    [Tooltip("Фаза игры, которую активирует этот объект")]
    public GamePhase phase;

    public GameObject activeTab;

    public GameObject unactiveTab;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        UpdateVisual(false);
    }

    public void UpdateVisual(bool isActive)
    {
        activeTab.SetActive(isActive);
        unactiveTab.SetActive(!isActive);
    }

    private void OnMouseDown()
    {
        GameManager.Instance.SwitchToPhase(phase);
    }

}
