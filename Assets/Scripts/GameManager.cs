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
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text passPanelCoinText;
    [SerializeField] private TMP_Text failPanelCoinText;

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
    private int sessionCoins = 0;
    [SerializeField] private TMP_Text tutorialCoinText;
    private void Awake()
    {
        // SINGLETON
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // RESET SESSION COINS
        sessionCoins = 0;
    }

    private void Start()
    {
        levelGameObject = GameObject.FindGameObjectWithTag("Level");
        levelPassPanel = FindInHierarchy("Pass");
        levelFailPanel = FindInHierarchy("Fail");

        tutorialCoinText = FindTextInHierarchy("TutCoin");
        Debug.Log("Level = " + levelGameObject);
        Debug.Log("Pass = " + levelPassPanel);
        Debug.Log("Fail = " + levelFailPanel);


        // FIND COUNTDOWN TEXTS — SCENE FULLY READY IN START
        countdownTexts = new List<TMP_Text>
        {
            FindTextInHierarchy("3"),
            FindTextInHierarchy("2"),
            FindTextInHierarchy("1"),
            FindTextInHierarchy("GO")
        };

        // FIND COIN UI
        GameObject coinObj = GameObject.Find("CoinTextHUD");

        if (coinObj != null)
        {
            coinText = coinObj.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.Log("No CoinsTextHUD found in this scene.");
        }

        passPanelCoinText = FindTextInHierarchy("CoinPass");
        failPanelCoinText = FindTextInHierarchy("CoinFail");

        // HOME BUTTONS
        if (passHomeButton) passHomeButton.onClick.AddListener(HomeButton);
        if (failHomeButton) failHomeButton.onClick.AddListener(HomeButton);
        if (pauseHomeButton) pauseHomeButton.onClick.AddListener(HomeButton);

        UpdateCoinHUD();

        StartCoroutine(LevelStartRoutine());
    }

    IEnumerator LevelStartRoutine()
    {
        Time.timeScale = 0f;
        gameStarted = false;
        Pauser.LockPause();

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
        Pauser.UnlockPause();
    }

    // ── COIN TRACKING ──────────────────────────────────────

    public void AddSessionCoin(int amount)
    {
        sessionCoins += amount;
        UpdateCoinHUD();
    }

    void UpdateCoinHUD()
    {
        if (coinText)
            coinText.text = sessionCoins.ToString();

        if (tutorialCoinText)
            tutorialCoinText.text = sessionCoins.ToString();
    }

    // ── GAME STATE ─────────────────────────────────────────

    public bool IsGameStarted() => gameStarted;

    // LEVEL PASS
    public void LevelPassed()
    {
        Time.timeScale = 0f;

        // LOCK PAUSE — HIDE PAUSE BUTTON AND BLOCK PAUSE
        Pauser.LockPause();

        // DON'T UNLOCK NEXT LEVEL IN TUTORIAL
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            int currentLevel = PlayerPrefs.GetInt(StringsData.levelToLoad, 1);
            int nextLevel = currentLevel + 1;
            int unlockedLevel = PlayerPrefs.GetInt(StringsData.playerLevel, 1);

            if (nextLevel > unlockedLevel)
            {
                PlayerPrefs.SetInt(StringsData.playerLevel, nextLevel);
                PlayerPrefs.Save();
            }
        }

        // COINS: collected + win bonus
        int totalCoins;

        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            // Tutorial: only collected coins, no bonus
            totalCoins = sessionCoins;
        }
        else
        {
            // Normal levels: collected coins + win bonus
            totalCoins = sessionCoins + winBonusCoins;
        }

        CurrecnyManager.instance.AddCurrency(totalCoins);

        if (passPanelCoinText)
            passPanelCoinText.text = "+" + totalCoins;

        if (passPanelCoinText)
            passPanelCoinText.text = "+" + totalCoins;

        // HIDE LEVEL, SHOW PASS PANEL
        if (levelGameObject) levelGameObject.SetActive(false);
        if (levelPassPanel) levelPassPanel.SetActive(true);

        PlayOneShot(levelPassClip);
    }

    // LEVEL FAIL
    public void LevelFailed()
    {
        Time.timeScale = 0f;

        // LOCK PAUSE — HIDE PAUSE BUTTON AND BLOCK PAUSE
        Pauser.LockPause();

        // COINS: only collected, no bonus
        CurrecnyManager.instance.AddCurrency(sessionCoins);

        if (failPanelCoinText)
            failPanelCoinText.text = "+" + sessionCoins;

        // HIDE LEVEL, SHOW FAIL PANEL
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
        Pauser.PauseLocked = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Pauser.PauseLocked = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void HomeButton()
    {
        Pauser.PauseLocked = false;
        Time.timeScale = 1f;

        PlayerPrefs.SetInt(StringsData.showSubscriptionPanel, 1);
        bool comingFromGame =
    PlayerPrefs.GetInt(StringsData.showSubscriptionPanel, 0) == 1;
        PlayerPrefs.Save();

        SceneManager.LoadScene(homeSceneName);
    }

   
    // SEARCHES ONLY LOADED SCENE CANVASES — NO PREFABS
    GameObject FindInHierarchy(string tag)
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            // SKIP IF NOT IN LOADED SCENE
            if (!canvas.gameObject.scene.isLoaded) continue;

            Transform[] children =
                canvas.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.CompareTag(tag))
                    return child.gameObject;
            }
        }

        return null;
    }

    // FIND ANY GAMEOBJECT BY TAG IN LOADED SCENE — NOT PREFABS
    GameObject FindInSceneByTag(string tag)
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in all)
        {
            if (obj.hideFlags == HideFlags.None &&
                obj.scene.isLoaded &&
                obj.CompareTag(tag))
                return obj;
        }

        Debug.LogWarning("GameManager: No GameObject found with tag '" + tag + "'");
        return null;
    }
    TMP_Text FindTextInHierarchy(string tag)
    {
        GameObject obj = FindInHierarchy(tag);

        if (obj != null)
        {
            TMP_Text txt = obj.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) return txt;
        }

        Debug.LogWarning("GameManager: No TMP_Text found with tag '" + tag + "'");
        return null;
    }

}