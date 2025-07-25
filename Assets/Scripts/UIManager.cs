using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UIManager : MonoBehaviour
{
    [Header("Элементы UI")]
    public Button resetButton;
    public TextMeshProUGUI questText; 

    void OnEnable()
    {
        GameManager.OnPhaseStateChanged += HandlePhaseStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnPhaseStateChanged -= HandlePhaseStateChanged;
    }

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
            resetButton.gameObject.SetActive(false);
        }
        UpdateQuestText(GamePhase.Acne);
    }

    private void HandlePhaseStateChanged(GamePhase phase, GameState state)
    {
        if (resetButton != null)
        {
            bool shouldShowResetButton = (state == GameState.PlayerControl);
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
            case GamePhase.Acne:
                textToShow = "УБЕРЕМ ПРЫЩИ";
                break;
            case GamePhase.Blush:
                textToShow = "НАНЕСЕМ РУМЯНА";
                break;
            case GamePhase.Eyeshadow:
                textToShow = "ДОБАВИМ ТЕНИ";
                break;
            case GamePhase.Lipstick:
                textToShow = "НАКРАСИМ ГУБЫ";
                break;
            default:
                textToShow = "КВЕСТ"; 
                break;
        }
        questText.text = textToShow;
    }

    private void OnResetButtonClicked()
    {
        GameManager.Instance.PerformReset();
    }
}