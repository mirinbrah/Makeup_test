using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Элементы UI")]
    public Button resetButton;
    public TextMeshProUGUI questText;

    [Header("Меню Паузы")]
    public Button menuButton;
    public GameObject pauseMenuPanel;

    void Awake()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }
    }

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        GameManager.OnPhaseStateChanged += HandlePhaseStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnPhaseStateChanged -= HandlePhaseStateChanged;
    }

    private void HandlePhaseStateChanged(GamePhase phase, GameState state)
    {
        if (resetButton != null)
        {
            bool shouldShowResetButton = (phase == GamePhase.Acne && state == GameState.PlayerControl);
            resetButton.gameObject.SetActive(shouldShowResetButton);
        }

        UpdateQuestText(phase);
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
        GameManager.Instance.PerformReset();
    }

    private void OnMenuButtonClicked()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }
}