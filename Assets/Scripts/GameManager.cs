using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject failPanel;

    void Awake()
    {
        // SINGLETON
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // HIDE PANEL AT START
        if (failPanel)
            failPanel.SetActive(false);
    }

    // SHOW FAIL PANEL
    public void LevelFailed()
    {
        if (failPanel)
            failPanel.SetActive(true);
    }

    // RESTART BUTTON
    public void RestartLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}