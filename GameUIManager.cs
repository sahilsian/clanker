using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject startMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    private bool isPaused = false;

    void Start()
    {
        // Show start menu at launch
        ShowStartMenu();
    }

    void Update()
    {
        // Pause toggle (e.g. Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // --- Menu states ---

    public void ShowStartMenu()
    {
        Time.timeScale = 0f;
        startMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(false);
        isPaused = false;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        startMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(true);
        isPaused = false;

        // TODO: reset player position/health/score as needed
    }

    public void TogglePause()
    {
        // Do not pause if in start menu or game over
        if (startMenuPanel.activeSelf || gameOverPanel.activeSelf)
            return;

        isPaused = !isPaused;

        pauseMenuPanel.SetActive(isPaused);
        hudPanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        startMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        isPaused = false;
    }

    // --- Button hooks ---

    public void OnStartButton()
    {
        StartGame();
    }

    public void OnResumeButton()
    {
        if (isPaused)
            TogglePause();
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitButton()
    {
        Application.Quit();

        // In Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnMainMenuButton()
    {
        ShowStartMenu();
    }
}
