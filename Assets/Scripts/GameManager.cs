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

    [Header("Fail Conditions")]
    [Tooltip("If the player falls below this Y, trigger a restart.")]
    public float fallDeathY = -7f;
    public Transform player;

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

        CheckFallDeath();
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

        // Reset fall flag when (re)entering gameplay flow
        if (newState == GameState.Start || newState == GameState.Playing)
            hasFallen = false;

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

    // -------- Fail Conditions --------

    private bool hasFallen;

    private void CheckFallDeath()
    {
        if (hasFallen) return;
        if (currentState != GameState.Playing) return;

        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player == null) return;

        if (player.position.y < fallDeathY)
        {
            hasFallen = true;
            // Disable the player if present, then show the Game Over UI so the player can restart
            player.gameObject.SetActive(false);
            SetState(GameState.GameOver);
        }
    }
}
