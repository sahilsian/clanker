using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject startMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.RegisterUI(this);
            }
        }
    }

    // --- Menu states ---

    public void ShowStartMenu()
    {
        SetPanelStates(false, true, false, false);
    }

    public void ShowHUD()
    {
        SetPanelStates(true, false, false, false);
    }

    public void ShowPauseMenu()
    {
        SetPanelStates(false, false, true, false);
    }

    public void ShowGameOver()
    {
        SetPanelStates(false, false, false, true);
    }

    private void SetPanelStates(bool showHud, bool showStart, bool showPause, bool showGameOver)
    {
        if (hudPanel != null)
            hudPanel.SetActive(showHud);
        if (startMenuPanel != null)
            startMenuPanel.SetActive(showStart);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(showPause);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(showGameOver);
    }

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    // --- Button hooks ---

    public void OnStartButton()
    {
        if (!EnsureManager())
            return;

        gameManager.StartGame();
    }

    public void OnResumeButton()
    {
        if (!EnsureManager())
            return;

        gameManager.ResumeGame();
    }

    public void OnRestartButton()
    {
        if (!EnsureManager())
            return;

        gameManager.RestartLevel();
    }

    public void OnQuitButton()
    {
        if (!EnsureManager())
            return;

        gameManager.QuitGame();
    }

    public void OnMainMenuButton()
    {
        if (!EnsureManager())
            return;

        gameManager.ReturnToMainMenu();
    }

    private bool EnsureManager()
    {
        if (gameManager != null)
            return true;

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RegisterUI(this);
            return true;
        }

        Debug.LogWarning("GameManager reference missing. Cannot process UI input.");
        return false;
    }
}
