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
    [Header("UI Groups")]
    public GameObject hudGroup;        // HUD_Group (health bar, etc.)
    public GameObject startScreen;     // StartScreen panel
    public GameObject pauseScreen;     // PauseScreen panel
    public GameObject gameOverScreen;  // GameOverScreen panel

    [Header("State")]
    public GameState currentState = GameState.Start;

    private void Start()
    {
        SetState(GameState.Start);
    }

    private void Update()
    {
        // Toggle Pause
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

        // Show screens based on state
        if (startScreen != null)
            startScreen.SetActive(newState == GameState.Start);

        if (pauseScreen != null)
            pauseScreen.SetActive(newState == GameState.Paused);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(newState == GameState.GameOver);

        if (hudGroup != null)
            hudGroup.SetActive(newState == GameState.Playing);

        // Time control
        switch (newState)
        {
            case GameState.Start:
            case GameState.Paused:
            case GameState.GameOver:
                Time.timeScale = 0f;   // freeze game
                break;

            case GameState.Playing:
                Time.timeScale = 1f;   // run game
                break;
        }
    }

    // -------- Button callbacks --------

    public void OnStartButton()
    {
        Debug.Log("StartButton clicked");
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

    // Call this from player when HP reaches 0
    public void ShowGameOver()
    {
        SetState(GameState.GameOver);
    }
}