using UnityEngine;
using UnityEngine.UI;

public class UIBootstrapper : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    private GameObject canvasGO;
    private GameObject hudPanel;
    private GameObject startPanel;
    private GameObject pausePanel;
    private GameObject gameOverPanel;
    private HealthBar healthBar;      // or SimpleHealthBar if that's what you use
    private GameManager gameManager;  // updated: use GameManager instead of GameUIManager

    void Awake()
    {
        // Build all UI elements and the GameManager at runtime
        CreateCanvas();
        CreatePanels();
        CreateHealthBar();
        CreateGameManager();
    }

    void CreateCanvas()
    {
        // Spawn a basic overlay canvas for all UI
        canvasGO = new GameObject("MainCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
    }

    void CreatePanels()
    {
        // Create UI panels for HUD, start, pause, and game over states
        // HUD
        hudPanel = CreatePanel("HUD_Panel", new Color(0, 0, 0, 0f));

        // Start menu
        startPanel = CreatePanel("StartMenu_Panel", new Color(0, 0, 0, 0.7f));

        // Pause menu
        pausePanel = CreatePanel("PauseMenu_Panel", new Color(0, 0, 0, 0.7f));
        pausePanel.SetActive(false);

        // Game over
        gameOverPanel = CreatePanel("GameOver_Panel", new Color(0, 0, 0, 0.8f));
        gameOverPanel.SetActive(false);
    }

    GameObject CreatePanel(string name, Color bgColor)
    {
        // Helper to generate a full-screen panel with a background color
        GameObject panelGO = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panelGO.GetComponent<Image>();
        img.color = bgColor;

        return panelGO;
    }

    void CreateHealthBar()
    {
        // Build a simple health bar UI and attach the HealthBar script
        // Background
        GameObject bgGO = new GameObject("HealthBar_BG", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(hudPanel.transform, false);

        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 1f);   // top-left
        bgRT.anchorMax = new Vector2(0f, 1f);
        bgRT.pivot = new Vector2(0f, 1f);
        bgRT.anchoredPosition = new Vector2(20f, -20f);
        bgRT.sizeDelta = new Vector2(300f, 30f);

        Image bgImage = bgGO.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f);

        // Fill
        GameObject fillGO = new GameObject("HealthBar_Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(bgGO.transform, false);

        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        Image fillImage = fillGO.GetComponent<Image>();
        fillImage.color = Color.red;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;

        // HealthBar logic
        healthBar = bgGO.AddComponent<HealthBar>();  // or SimpleHealthBar
        healthBar.fillImage = fillImage;
        healthBar.maxHealth = maxHealth;
        healthBar.SetHealth(maxHealth);              // adjust if your class uses a different API
    }

    void CreateGameManager()
    {
        // Instantiate GameManager and hook up the runtime-generated UI references
        GameObject gmGO = new GameObject("GameManagerObject");
        gameManager = gmGO.AddComponent<GameManager>();

        // Wire the runtime-created panels into GameManager
        gameManager.hudGroup      = hudPanel;
        gameManager.startScreen   = startPanel;
        gameManager.pauseScreen   = pausePanel;
        gameManager.gameOverScreen = gameOverPanel;
    }
}
