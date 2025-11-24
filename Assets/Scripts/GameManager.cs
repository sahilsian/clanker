using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Groups")]
    public GameObject hudGroup;
    public GameObject startScreen;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;

    [Header("UI Elements")]
    public GameObject healthBar;   // NEW — separate health bar toggle

    [Header("State")]
    public GameState currentState = GameState.Start;

    // NEW — Dialogue freeze flag
    public bool isDialogueActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.Start);
    }

    private void Update()
    {
        // Disable pause during dialogue
        if (isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (currentState == GameState.Paused)
                SetState(GameState.Playing);
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        if (startScreen != null)
            startScreen.SetActive(newState == GameState.Start);

        if (pauseScreen != null)
            pauseScreen.SetActive(newState == GameState.Paused);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(newState == GameState.GameOver);

        if (hudGroup != null)
            hudGroup.SetActive(newState == GameState.Playing);

        // NEW — Enable Health Bar ONLY while playing
        if (healthBar != null)
            healthBar.SetActive(newState == GameState.Playing);

        switch (newState)
        {
            case GameState.Start:
            case GameState.Paused:
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;
        }
    }

    // -------- Dialogue Hooks --------

    public void BeginDialogue()
    {
        isDialogueActive = true;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
    }

    // -------- Button Callbacks --------

    public void OnStartButton()
    {
        SetState(GameState.Playing);
    }

    public void OnResumeButton()
    {
        SetState(GameState.Playing);
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowGameOver()
    {
        SetState(GameState.GameOver);
    }
}
