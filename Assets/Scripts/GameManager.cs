using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string homeSceneName = "Home";

    [Header("Level Start Pause")]
    [SerializeField] private float startPauseDuration = 3f;

    [Header("Countdown UI (Order matters)")]
    [Tooltip("Index 0 = 3, 1 = 2, 2 = 1, 3 = GO")]
    [SerializeField] private List<TMP_Text> countdownTexts;

    [Header("Level UI")]
    public GameObject levelGameObject;
    public GameObject levelPassPanel;
    public GameObject levelFailPanel;

    [Header("Audio")]
    public AudioClip levelPassClip;
    public AudioClip levelFailClip;

    private bool gameStarted = false;

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

        // HIDE PANELS
        if (levelPassPanel)
            levelPassPanel.SetActive(false);

        if (levelFailPanel)
            levelFailPanel.SetActive(false);

        // SHOW LEVEL UI
        if (levelGameObject)
            levelGameObject.SetActive(true);
    }

    GameObject FindObjectByTagIncludingInactive(string tag)
    {
        GameObject[] allObjects =
            Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(tag))
            {
                return obj;
            }
        }

        return null;
    }

    private void Start()
    {
        StartCoroutine(LevelStartRoutine());
    }

    IEnumerator LevelStartRoutine()
    {
        // PAUSE GAME
        Time.timeScale = 0f;
        gameStarted = false;

        // HIDE COUNTDOWN
        foreach (TMP_Text txt in countdownTexts)
        {
            if (txt)
                txt.gameObject.SetActive(false);
        }

        int seconds = Mathf.CeilToInt(startPauseDuration);

        // COUNTDOWN LOOP
        for (int i = 0; i < seconds && i < countdownTexts.Count; i++)
        {
            countdownTexts[i].gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(1f);

            countdownTexts[i].gameObject.SetActive(false);
        }

        // GO
        int goIndex = seconds;

        if (goIndex < countdownTexts.Count)
        {
            countdownTexts[goIndex].gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(0.5f);

            countdownTexts[goIndex].gameObject.SetActive(false);
        }

        // RESUME GAME
        Time.timeScale = 1f;
        gameStarted = true;
    }

    // CHECK GAME STARTED
    public bool IsGameStarted()
    {
        return gameStarted;
    }

    // LEVEL PASS
    public void LevelPassed()
    {
        Time.timeScale = 0f;

        if (levelGameObject)
            levelGameObject.SetActive(false);

        if (levelPassPanel)
            levelPassPanel.SetActive(true);

        PlayOneShot(levelPassClip);
    }

    // LEVEL FAIL
    public void LevelFailed()
    {
        Time.timeScale = 0f;

        if (levelGameObject)
            levelGameObject.SetActive(false);

        if (levelFailPanel)
            levelFailPanel.SetActive(true);

        PlayOneShot(levelFailClip);
    }

    // PLAY ONE SHOT AUDIO
    void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        GameObject audioObj =
            new GameObject("OneShotAudio");

        AudioSource source =
            audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.Play();

        Destroy(audioObj, clip.length);
    }

    // RESTART LEVEL
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // NEXT LEVEL
    public void NextLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex + 1
        );
    }

    // HOME BUTTON
    public void HomeButton()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(homeSceneName);
    }
}