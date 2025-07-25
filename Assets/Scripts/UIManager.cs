using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public GamePhase phase;
        public Button button;
        public Image buttonImage;
        public Sprite activeSprite;
        [HideInInspector]
        public Sprite inactiveSprite;
    }

    [Header("Элементы UI")]
    public Button resetButton;
    public TextMeshProUGUI questText;

    [Header("Вкладки")]
    public GameObject creamTabVisual;
    public GameObject makeupTabsContainer;
    public List<Tab> makeupTabs;

    void Awake()
    {
        Debug.Log("UIManager: Awake. Настраиваю вкладки и кнопки.");
        foreach (var tab in makeupTabs)
        {
            if (tab.buttonImage != null)
            {
                tab.inactiveSprite = tab.buttonImage.sprite;
            }

            if (tab.button != null)
            {
                GamePhase phaseToCall = tab.phase;
                tab.button.onClick.AddListener(() => OnTabClicked(phaseToCall));
            }
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }

    void OnEnable()
    {
        Debug.Log("UIManager: OnEnable. Подписываюсь на событие GameManager.OnPhaseStateChanged.");
        GameManager.OnPhaseStateChanged += HandlePhaseStateChanged;
    }

    void OnDisable()
    {
        Debug.Log("UIManager: OnDisable. Отписываюсь от события.");
        GameManager.OnPhaseStateChanged -= HandlePhaseStateChanged;
    }

    void Start()
    {
        Debug.Log("UIManager: Start. Вызываю HandlePhaseStateChanged для начальной настройки.");
        HandlePhaseStateChanged(GamePhase.Acne, GameState.Idle);
    }

    private void OnTabClicked(GamePhase phase)
    {
        // ЭТОТ ЛОГ ДОЛЖЕН ПОЯВИТЬСЯ ПРИ НАЖАТИИ НА КНОПКУ
        Debug.Log("UIManager: >>> КНОПКА НАЖАТА. Вызываю OnTabClicked для фазы: " + phase);
        GameManager.Instance.SwitchToPhase(phase);
    }

    private void HandlePhaseStateChanged(GamePhase phase, GameState state)
    {
        // ЭТОТ ЛОГ НЕ ПОЯВИТСЯ ПОСЛЕ НАЖАТИЯ, ПОТОМУ ЧТО GAMEMANAGER ЗАБЛОКИРОВАН
        Debug.Log("UIManager: <<< ПОЛУЧЕНО СОБЫТИЕ. Фаза: " + phase + ", Состояние: " + state);

        if (resetButton != null)
        {
            bool shouldShowResetButton = (phase == GamePhase.Acne && state == GameState.PlayerControl);
            resetButton.gameObject.SetActive(shouldShowResetButton);
        }

        bool isCreamPhase = (phase == GamePhase.Acne);
        creamTabVisual.SetActive(isCreamPhase);
        makeupTabsContainer.SetActive(!isCreamPhase);

        UpdateQuestText(phase);

        if (!isCreamPhase)
        {
            UpdateSelectedTab(phase);
        }
    }

    private void UpdateSelectedTab(GamePhase selectedPhase)
    {
        Debug.Log("UIManager: Обновляю активную вкладку. Новая фаза: " + selectedPhase);
        foreach (var tab in makeupTabs)
        {
            if (tab.buttonImage != null)
            {
                tab.buttonImage.sprite = (tab.phase == selectedPhase) ? tab.activeSprite : tab.inactiveSprite;
            }
        }
    }

    private void UpdateQuestText(GamePhase newPhase)
    {
        if (questText == null) return;
        string textToShow = "";
        switch (newPhase)
        {
            case GamePhase.Acne: textToShow = "УБЕРЕМ\nАКНЕ"; break;
            case GamePhase.Blush: textToShow = "НАНЕСЕМ\nРУМЯНА"; break;
            case GamePhase.Eyeshadow: textToShow = "ДОБАВИМ\nТЕНИ"; break;
            case GamePhase.Lipstick: textToShow = "НАКРАСИМ\nГУБЫ"; break;
            default: textToShow = "КВЕСТ"; break;
        }
        questText.text = textToShow;
    }

    private void OnResetButtonClicked()
    {
        Debug.Log("UIManager: Кнопка Reset нажата.");
        GameManager.Instance.PerformReset();
    }
}