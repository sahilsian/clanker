using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        StartMenu,
        Playing,
        Paused,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameUIManager uiManager;

    public GameState CurrentState { get; private set; } = GameState.StartMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<GameUIManager>();
        }

        if (uiManager != null)
        {
            uiManager.Initialize(this);
        }
        else
        {
            Debug.LogError("GameUIManager not found in the scene.");
        }

        EnterStartMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void RegisterUI(GameUIManager manager)
    {
        uiManager = manager;
        uiManager.Initialize(this);
    }

    private void EnterStartMenu()
    {
        CurrentState = GameState.StartMenu;
        Time.timeScale = 0f;
        uiManager?.ShowStartMenu();
    }

    public void StartGame()
    {
        if (CurrentState == GameState.Playing)
            return;

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        uiManager?.ShowHUD();
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.StartMenu || CurrentState == GameState.GameOver)
            return;

        if (CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        uiManager?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
            return;

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        uiManager?.ShowHUD();
    }

    public void TriggerGameOver()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        uiManager?.ShowGameOver();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        EnterStartMenu();
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
