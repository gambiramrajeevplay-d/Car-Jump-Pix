using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string homeSceneName = "UI";

    [Header("Level Start Pause")]
    [SerializeField] private float startPauseDuration = 3f;

    [Header("Countdown UI (Order matters)")]
    [Tooltip("Index 0 = 3, 1 = 2, 2 = 1, 3 = GO")]
    [SerializeField] private List<TMP_Text> countdownTexts;

    [Header("Level UI")]
    public GameObject levelGameObject;
    public GameObject levelPassPanel;
    public GameObject levelFailPanel;

    [Header("Coin UI")]
    [SerializeField] private TMP_Text coinText;           // HUD coin text (during gameplay)
    [SerializeField] private TMP_Text passPanelCoinText;  // Coin text on Pass panel
    [SerializeField] private TMP_Text failPanelCoinText;  // Coin text on Fail panel

    [Header("Win Bonus")]
    [SerializeField] private int winBonusCoins = 100;

    [Header("Home Buttons")]
    public UnityEngine.UI.Button passHomeButton;
    public UnityEngine.UI.Button failHomeButton;
    public UnityEngine.UI.Button pauseHomeButton;

    [Header("Audio")]
    public AudioClip levelPassClip;
    public AudioClip levelFailClip;

    private bool gameStarted = false;
    private int sessionCoins = 0;  // Coins collected this level

    private void Awake()
    {
        // SINGLETON
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        levelGameObject = FindObjectByTagIncludingInactive("Level");
        levelPassPanel = FindObjectByTagIncludingInactive("Pass");
        levelFailPanel = FindObjectByTagIncludingInactive("Fail");

        // AUTO-FIND COUNTDOWN TEXTS BY TAG
        countdownTexts = new List<TMP_Text>
    {
        FindTextByTag("3"),
        FindTextByTag("2"),
        FindTextByTag("1"),
        FindTextByTag("GO")
    };

        // AUTO-FIND COIN UI BY TAG
        // FIND BY NAME IN CHILDREN OR SCENE
        GameObject coinObj = GameObject.Find("CoinsTextHUD");
        if (coinObj != null)
            coinText = coinObj.GetComponent<TMP_Text>();
        passPanelCoinText = FindTextByTag("CoinPass");
        failPanelCoinText = FindTextByTag("CoinFail");

        // AUTO-FIND HOME BUTTONS AND ADD LISTENERS
        // HOME BUTTONS
        if (passHomeButton) passHomeButton.onClick.AddListener(HomeButton);
        if (failHomeButton) failHomeButton.onClick.AddListener(HomeButton);
        if (pauseHomeButton) pauseHomeButton.onClick.AddListener(HomeButton);

        // HIDE PANELS
        if (levelPassPanel) levelPassPanel.SetActive(false);
        if (levelFailPanel) levelFailPanel.SetActive(false);

        // SHOW LEVEL UI
        if (levelGameObject) levelGameObject.SetActive(true);

        // RESET SESSION COINS
        sessionCoins = 0;
        UpdateCoinHUD();
    }

    // FIND TMP_TEXT BY TAG
    TMP_Text FindTextByTag(string tag)
    {
        GameObject obj = FindObjectByTagIncludingInactive(tag);

        if (obj != null)
        {
            // CHECK SELF FIRST, THEN CHILDREN
            TMP_Text txt = obj.GetComponentInChildren<TMP_Text>(true);

            if (txt != null)
                return txt;
        }

        Debug.LogWarning("GameManager: No TMP_Text found with tag '" + tag + "'");
        return null;
    }
    // ADD HOME BUTTON LISTENER BY TAG
    void AddHomeButtonListener(string tag)
    {
        GameObject obj = FindObjectByTagIncludingInactive(tag);

        if (obj != null)
        {
            UnityEngine.UI.Button btn = obj.GetComponent<UnityEngine.UI.Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(HomeButton);
            }
            else
            {
                Debug.LogWarning("GameManager: No Button component on tagged object '" + tag + "'");
            }
        }
        else
        {
            Debug.LogWarning("GameManager: No GameObject found with tag '" + tag + "'");
        }
    }
    GameObject FindObjectByTagIncludingInactive(string tag)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(tag))
                return obj;
        }

        return null;
    }

    private void Start()
    {
        StartCoroutine(LevelStartRoutine());
    }

    IEnumerator LevelStartRoutine()
    {
        Time.timeScale = 0f;
        gameStarted = false;

        foreach (TMP_Text txt in countdownTexts)
        {
            if (txt) txt.gameObject.SetActive(false);
        }

        int seconds = Mathf.CeilToInt(startPauseDuration);

        for (int i = 0; i < seconds && i < countdownTexts.Count; i++)
        {
            countdownTexts[i].gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            countdownTexts[i].gameObject.SetActive(false);
        }

        int goIndex = seconds;

        if (goIndex < countdownTexts.Count)
        {
            countdownTexts[goIndex].gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.5f);
            countdownTexts[goIndex].gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
        gameStarted = true;
    }

    // ── COIN TRACKING ──────────────────────────────────────

    // Called by CoinPickUp
    public void AddSessionCoin(int amount)
    {
        sessionCoins += amount;
        UpdateCoinHUD();
    }

    void UpdateCoinHUD()
    {
        if (coinText)
            coinText.text = sessionCoins.ToString();
    }
    // ── GAME STATE ─────────────────────────────────────────

    public bool IsGameStarted() => gameStarted;

    // LEVEL PASS — collected coins + 100 bonus
    public void LevelPassed()
    {
        Time.timeScale = 0f;

        // UNLOCK NEXT LEVEL
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = currentBuildIndex + 1;
        int highestUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (nextLevelIndex > highestUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevelIndex);
            PlayerPrefs.Save();
        }

        // COINS: collected + win bonus
        int totalCoins = sessionCoins + winBonusCoins;
        CurrecnyManager.instance.AddCurrency(totalCoins);

        // UPDATE PASS PANEL TEXT
        if (passPanelCoinText)
            passPanelCoinText.text = "+" + totalCoins;

        if (levelGameObject) levelGameObject.SetActive(false);
        if (levelPassPanel) levelPassPanel.SetActive(true);

        PlayOneShot(levelPassClip);
    }

    // LEVEL FAIL — only collected coins
    public void LevelFailed()
    {
        Time.timeScale = 0f;

        // COINS: only what was collected, no bonus
        CurrecnyManager.instance.AddCurrency(sessionCoins);

        // UPDATE FAIL PANEL TEXT
        if (failPanelCoinText)
            failPanelCoinText.text = "+" + sessionCoins;

        if (levelGameObject) levelGameObject.SetActive(false);
        if (levelFailPanel) levelFailPanel.SetActive(true);

        PlayOneShot(levelFailClip);
    }

    // ── AUDIO ──────────────────────────────────────────────

    void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;

        GameObject audioObj = new GameObject("OneShotAudio");
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        Destroy(audioObj, clip.length);
    }

    // ── NAVIGATION ─────────────────────────────────────────

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void HomeButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }
}