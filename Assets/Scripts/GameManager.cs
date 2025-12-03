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
    public GameObject healthBar;
    [Header("Level Settings")]
    public bool skipStartScreen = false;
    public bool YouWin = false;
    public bool boss = false;
    [Header("Fail Conditions")]
    [Tooltip("If the player falls below this Y, trigger a restart.")]
    public float fallDeathY = -7f;
    public Transform player;
    [Header("Enemy + Portal System")]
    public GameObject portalPrefab;       
    public Transform portalSpawnPoint;    
    [Header("State")]
    public GameState currentState = GameState.Start;

    public bool isDialogueActive = false;
    private int totalEnemies;
    private int defeatedEnemies = 0;
    private bool portalSpawned = false;

    private void Awake()
    {
        // Establish singleton reference for other scripts
        Instance = this;
    }

    private void Start()
    {
        
        // Begin at the start screen
        if (skipStartScreen)
        {
            SetState(GameState.Playing);
        }
        else
        {
            SetState(GameState.Start);
        }
        
        EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        totalEnemies = enemies.Length;
        if (boss)
        {
            totalEnemies++;
        }
        Debug.Log("Total enemies found: " + totalEnemies);

    }

    private void Update()
    {
        // Handle pause toggles and death checks unless dialogue is active
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
        // Swap UI and timescale based on the requested game state
        currentState = newState;

        if (startScreen != null)
            startScreen.SetActive(newState == GameState.Start);

        if (pauseScreen != null)
            pauseScreen.SetActive(newState == GameState.Paused);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(newState == GameState.GameOver);

        if (hudGroup != null)
            hudGroup.SetActive(newState == GameState.Playing);

        if (healthBar != null)
            healthBar.SetActive(newState == GameState.Playing);

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
        // Pause gameplay input reactions while dialogue is shown
        isDialogueActive = true;
    }

    public void EndDialogue()
    {
        // Resume gameplay input reactions after dialogue closes
        isDialogueActive = false;
    }

    // -------- Button Callbacks --------

    public void OnStartButton()
    {
        // Start button from start menu
        SetState(GameState.Playing);
    }

    public void OnResumeButton()
    {
        // Resume button from pause menu
        SetState(GameState.Playing);
    }

    public void OnRestartButton()
    {
        // Reload current scene and unpause time
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowGameOver()
    {
        // External trigger to enter the game over state
        SetState(GameState.GameOver);
    }

    // -------- Fail Conditions --------

    private bool hasFallen;

    private void CheckFallDeath()
    {
        // Detect if the player has fallen below the threshold and end the game
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
    // -------- Enemy System --------
    public void EnemyDefeated()
    {
        defeatedEnemies++;
        Debug.Log("GameManager received enemy death. Count: " + defeatedEnemies + "/" + totalEnemies);

        if (defeatedEnemies >= totalEnemies && !portalSpawned && YouWin == false)
        {
            SpawnPortal();
        }
        else if (defeatedEnemies >= totalEnemies && !portalSpawned && YouWin == true) { 
            GoToGameOverScene();
        }
    }

    private void SpawnPortal()
    {
        portalSpawned = true;

        if (portalPrefab != null && portalSpawnPoint != null)
        {
            Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Portal prefab or spawn point not assigned!");
        }
    }
    public void GoToGameOverScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("youWIN");
    }
}
