using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public Button continueButton;
    public Button newGame;
    public Button quitButton;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public Button dialogueNextButton;
    public float typingSpeed = 0.03f;
    public GameObject blackPanel;
    public Image banner;
    public Image border;

    private string[] dialoguePages = new string[]
    {
        "Autumn has arrived, and the forest spirits prepare for their long winter rest...",
        "Before they fall asleep, they seek a challenger worthy enough to best them in their sacred trials.",
        "Be careful, Rogue. For all may not be as it seems."
    };

    private int currentPageIndex = 0;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        RefreshContinueButton();

        if (dialogueNextButton != null)
        {
            dialogueNextButton.onClick.RemoveAllListeners();
            dialogueNextButton.onClick.AddListener(OnNextDialogueClicked);
        }
    }

    public void RefreshContinueButton()
    {
        bool hasActiveRun = PlayerPrefs.GetInt("HasActiveRun", 0) == 1;

        if (continueButton != null)
        {
            continueButton.interactable = hasActiveRun;
            
            TMP_Text btnText = continueButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.color = hasActiveRun ? Color.white : new Color(1f, 1f, 1f, 0.35f); 
            }
            
        }
    }

    public void OnNewGameClicked()
    {
        SoundManager.Instance?.PlayButtonClick();           
        PlayerPrefs.DeleteKey("HasActiveRun");
        PlayerPrefs.DeleteKey("CurrentAnte");
        PlayerPrefs.DeleteKey("PlayerMoney");
        PlayerPrefs.DeleteKey("EquippedSlot1");
        PlayerPrefs.DeleteKey("EquippedSlot2");
        PlayerPrefs.DeleteKey("SavedPurchasedAbilities");

        if (newGame != null) newGame.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        banner.enabled = false;
        border.enabled = false;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            currentPageIndex = 0;
            StartPage(currentPageIndex);
        }
        else
        {
            StartNewRun();
        }
    }

    public void OnContinueClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        string sceneToLoad = PlayerPrefs.GetString("LastScene", "SampleScene"); 
        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); 
        }
    }

    void StartPage(int index)
    { 
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypeWriterText(dialoguePages[index]));
    }

    IEnumerator TypeWriterText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void OnNextDialogueClicked()
    {
        SoundManager.Instance?.PlayButtonClick();

        if (isTyping)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueText.text = dialoguePages[currentPageIndex];
            isTyping = false;
            return;
        }

        currentPageIndex++;

        if (currentPageIndex < dialoguePages.Length)
        {
            StartPage(currentPageIndex);
        }
        else
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            StartNewRun();
        }
    }

    private void StartNewRun()
    {
        PlayerPrefs.SetInt("HasActiveRun", 1);
        PlayerPrefs.SetInt("CurrentAnte", 1);
        PlayerPrefs.SetInt("PlayerMoney", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("SampleScene");
    }
    public void Quit()
    {
        blackPanel.SetActive(true);
        #if UNITY_WEBGL
        Screen.fullScreen = false;
        #else
        Application.Quit();
        #endif
    }
}