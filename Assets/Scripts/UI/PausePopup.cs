using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PausePopup : MonoBehaviour
{
    [Header("Кнопки меню")]
    public Button continueButton;
    public Button exitButton;

    public GameObject blocker;

    void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }

    private void OnEnable()
    {
        if (blocker != null)
        {
            blocker.SetActive(true);
        }
        GameManager.Instance.PauseGame();
    }

    private void OnContinueButtonClicked()
    {
        if (blocker != null)
        {
            blocker.SetActive(false);
        }
        gameObject.SetActive(false);

        GameManager.Instance.ResumeGame();
    }

    private void OnExitButtonClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}