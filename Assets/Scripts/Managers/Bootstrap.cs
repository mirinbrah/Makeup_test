using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Bootstrap : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI upperText;
    [SerializeField] private TextMeshProUGUI websiteText;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI githubText;
    [SerializeField] private TextMeshProUGUI sdkText;
    [SerializeField] private Button playButton;

    [Header("Тексты")]
    [SerializeField] private string sdkLoadedText = "СДК загружен"; // <-- НОВАЯ ПЕРЕМЕННАЯ

    [Header("Задержки")]
    [SerializeField] private float delayBetweenTexts = 1.0f;
    [SerializeField] private float sdkLoadDuration = 3.0f;

    private void Start()
    {
        playButton.onClick.AddListener(LoadNextScene);
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(delayBetweenTexts);
        upperText.gameObject.SetActive(true);

        yield return new WaitForSeconds(delayBetweenTexts);
        websiteText.gameObject.SetActive(true);

        yield return new WaitForSeconds(delayBetweenTexts);
        authorText.gameObject.SetActive(true);

        yield return new WaitForSeconds(delayBetweenTexts);
        githubText.gameObject.SetActive(true);

        yield return new WaitForSeconds(delayBetweenTexts);

        sdkText.gameObject.SetActive(true);
        string initialSdkText = sdkText.text;
        float timer = sdkLoadDuration;

        while (timer > 0)
        {
            sdkText.text = $"{initialSdkText} {Mathf.CeilToInt(timer)}...";
            timer -= Time.deltaTime;
            yield return null;
        }

        sdkText.text = sdkLoadedText;

        playButton.gameObject.SetActive(true);
    }

    private void LoadNextScene()
    {
        playButton.interactable = false;
        SceneManager.LoadScene(1);
    }

    public void OpenWebsiteURL()
    {
        Application.OpenURL("https://playnera.com");
    }

    public void OpenGithubURL()
    {
        Application.OpenURL("https://github.com/mirinbrah");
    }
}