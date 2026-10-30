using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Every screen outside the arena: start menu, controls, shop, pause, game over / win.
// Lives on the "Menus" canvas built by Tools > Dodge Game > Set Up Menus.
// The menus are in the main scene: Restart reloads straight into a run, Main Menu reloads to the menu.
public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopColour
    {
        public string id;
        public string displayName;
        public int price;
        public Color color;

        public ShopColour() { }

        public ShopColour(string id, string displayName, int price, Color color)
        {
            this.id = id;
            this.displayName = displayName;
            this.price = price;
            this.color = color;
        }
    }

    enum State { MainMenu, Playing, Paused, Ended }

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;
    public GameObject shopPanel;
    public GameObject pausePanel;
    public GameObject endPanel;
    public Button pauseButton;

    [Header("Main Menu")]
    public Button playButton;
    public Button shopButton;
    public Button controlsButton;
    public Button quitButton;
    public TextMeshProUGUI menuOrbsText;

    [Header("Controls")]
    public Button controlsBackButton;

    [Header("Pause")]
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;

    [Header("Game Over / Win")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI orbsEarnedText;
    public Button endRestartButton;
    public Button endMainMenuButton;

    [Header("Shop")]
    public Button shopBackButton;
    public TextMeshProUGUI shopOrbsText;
    public RectTransform colourGrid;
    public RectTransform skinGrid;
    // The first colour is the default: free, and leaves the model's own colours untouched.
    public ShopColour[] colours =
    {
        new ShopColour("default", "Classic Blue", 0, new Color(0.10f, 0.35f, 0.85f)),
        new ShopColour("red", "Red", 5, new Color(0.85f, 0.12f, 0.12f)),
        new ShopColour("green", "Green", 10, new Color(0.15f, 0.75f, 0.25f)),
        new ShopColour("purple", "Purple", 15, new Color(0.55f, 0.2f, 0.85f)),
        new ShopColour("gold", "Gold", 25, new Color(1f, 0.75f, 0.1f)),
        new ShopColour("black", "Shadow", 40, new Color(0.08f, 0.08f, 0.1f)),
    };
    // Skins need new models, so they are shown as locked until those exist.
    public string[] upcomingSkins = { "Robot Suit", "Ninja", "Astronaut" };
    [Tooltip("Material on the player model that the shop colour replaces.")]
    public string suitMaterialName = "Player_Suit";

    // Survives the scene reload so Restart skips the start menu.
    static bool skipMenuOnLoad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        // Domain reload is off in the editor, so statics must be cleared for each Play.
        skipMenuOnLoad = false;
    }

    private State state;
    private GameManager gameManager;
    private PlayerMovement player;

    void Awake()
    {
        playButton.onClick.AddListener(StartRun);
        shopButton.onClick.AddListener(OpenShop);
        controlsButton.onClick.AddListener(() => ShowOnly(controlsPanel));
        quitButton.onClick.AddListener(Quit);
        controlsBackButton.onClick.AddListener(ShowMainMenu);
        shopBackButton.onClick.AddListener(ShowMainMenu);

        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        pauseRestartButton.onClick.AddListener(Restart);
        pauseMainMenuButton.onClick.AddListener(MainMenu);

        endRestartButton.onClick.AddListener(Restart);
        endMainMenuButton.onClick.AddListener(MainMenu);
    }

    // Called by GameManager.Start once the scene is set up.
    public void OnSceneStarted(GameManager manager)
    {
        gameManager = manager;
        player = FindFirstObjectByType<PlayerMovement>();

        if (skipMenuOnLoad)
        {
            skipMenuOnLoad = false;
            StartRun();
        }
        else
        {
            ShowMainMenu();
        }
    }

    void Update()
    {
        // Esc on PC, the Back button on Android.
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (state == State.Playing)
            Pause();
        else if (state == State.Paused)
            Resume();
        else if (state == State.MainMenu && !mainMenuPanel.activeSelf)
            ShowMainMenu();
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused)
            return;

        ShopData.Save();

        // Switching away from the app on a phone pauses the run.
        if (state == State.Playing)
            Pause();
    }

    // =============================================
    // SCREENS
    // =============================================

    void ShowOnly(GameObject panel)
    {
        mainMenuPanel.SetActive(panel == mainMenuPanel);
        controlsPanel.SetActive(panel == controlsPanel);
        shopPanel.SetActive(panel == shopPanel);
        pausePanel.SetActive(panel == pausePanel);
        endPanel.SetActive(panel == endPanel);
        pauseButton.gameObject.SetActive(state == State.Playing);
    }

    void ShowMainMenu()
    {
        state = State.MainMenu;
        Freeze(true);
        menuOrbsText.text = "Orbs: " + ShopData.Orbs;
        ShowOnly(mainMenuPanel);
    }

    void StartRun()
    {
        state = State.Playing;
        ApplyEquippedColour();
        Freeze(false);
        ShowOnly(null);
    }

    void Pause()
    {
        if (state != State.Playing)
            return;

        state = State.Paused;
        Freeze(true);
        ShowOnly(pausePanel);
    }

    void Resume()
    {
        if (state != State.Paused)
            return;

        state = State.Playing;
        Freeze(false);
        ShowOnly(null);
    }

    // Called by GameManager when the player dies or wins. GameManager freezes time itself.
    public void ShowEndScreen(bool won, int score, int orbsThisRun)
    {
        state = State.Ended;

        if (player != null)
            player.enabled = false;

        resultText.text = won ? "YOU WIN!" : "GAME OVER";
        resultText.color = won ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.3f, 0.25f);
        finalScoreText.text = "Score: " + score + "     Best: " + GameManager.HighScore;
        orbsEarnedText.text = "Orbs collected: " + orbsThisRun + "   (total " + ShopData.Orbs + ")";

        ShowOnly(endPanel);
    }

    void Restart()
    {
        skipMenuOnLoad = true;
        gameManager.RestartGame();
    }

    void MainMenu()
    {
        skipMenuOnLoad = false;
        gameManager.RestartGame();
    }

    void Quit()
    {
        ShopData.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Stops everything (time, physics, player input) while a menu is open.
    void Freeze(bool frozen)
    {
        Time.timeScale = frozen ? 0f : 1f;

        // Also stops a keyboard flip being queued while paused and firing on Resume.
        if (player != null)
            player.enabled = !frozen;
    }

    // =============================================
    // SHOP
    // =============================================

    void OpenShop()
    {
        RefreshShop();
        ShowOnly(shopPanel);
    }

    ShopColour EquippedColour()
    {
        string id = ShopData.GetEquipped(colours[0].id);

        foreach (ShopColour colour in colours)
        {
            if (colour.id == id && ShopData.IsOwned(colour.id, colour.price))
                return colour;
        }

        return colours[0];
    }

    void RefreshShop()
    {
        shopOrbsText.text = "Orbs: " + ShopData.Orbs;
        string equippedId = EquippedColour().id;

        // Hidden first so the grid lays out the new cards this frame, not next to the old ones.
        foreach (Transform child in colourGrid)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        foreach (ShopColour colour in colours)
            AddColourCard(colour, colour.id == equippedId);

        if (skinGrid.childCount == 0)
        {
            foreach (string skin in upcomingSkins)
                AddSkinCard(skin);
        }
    }

    void AddColourCard(ShopColour colour, bool equipped)
    {
        Vector2 middle = new Vector2(0.5f, 0.5f);
        Image card = UIFactory.Box(colourGrid, colour.displayName, middle, Vector2.zero, Vector2.zero,
                                   new Color(0.12f, 0.14f, 0.22f));

        UIFactory.Box(card.transform, "Swatch", middle, new Vector2(0f, 60f), new Vector2(120f, 120f), colour.color);
        UIFactory.Text(card.transform, "Name", colour.displayName, 30f, middle, new Vector2(0f, -25f),
                       new Vector2(210f, 40f), Color.white);

        bool owned = ShopData.IsOwned(colour.id, colour.price);
        string label;
        Color buttonColour;

        if (equipped)
        {
            label = "EQUIPPED";
            buttonColour = new Color(0.2f, 0.6f, 0.3f);
        }
        else if (owned)
        {
            label = "EQUIP";
            buttonColour = new Color(0.15f, 0.4f, 0.9f);
        }
        else
        {
            label = "BUY  " + colour.price;
            buttonColour = ShopData.Orbs >= colour.price ? new Color(1f, 0.55f, 0.05f) : new Color(0.35f, 0.35f, 0.4f);
        }

        Button button = UIFactory.Button(card.transform, "Button", label, middle, new Vector2(0f, -85f),
                                         new Vector2(190f, 60f), buttonColour, 30f);
        button.interactable = !equipped;
        button.onClick.AddListener(() => OnColourPressed(colour));
    }

    void OnColourPressed(ShopColour colour)
    {
        if (ShopData.IsOwned(colour.id, colour.price))
            ShopData.Equip(colour.id);
        else if (ShopData.TryBuy(colour.id, colour.price))
            ShopData.Equip(colour.id);

        RefreshShop();
    }

    void AddSkinCard(string skinName)
    {
        Vector2 middle = new Vector2(0.5f, 0.5f);
        Image card = UIFactory.Box(skinGrid, skinName, middle, Vector2.zero, Vector2.zero,
                                   new Color(0.12f, 0.14f, 0.22f, 0.7f));

        UIFactory.Box(card.transform, "Silhouette", middle, new Vector2(0f, 45f), new Vector2(90f, 90f),
                      new Color(0.3f, 0.3f, 0.35f));
        UIFactory.Text(card.transform, "Lock", "?", 60f, middle, new Vector2(0f, 45f), new Vector2(90f, 90f),
                       new Color(0.6f, 0.6f, 0.65f));
        UIFactory.Text(card.transform, "Name", skinName, 30f, middle, new Vector2(0f, -25f),
                       new Vector2(280f, 40f), new Color(0.75f, 0.75f, 0.8f));
        UIFactory.Text(card.transform, "Status", "STILL IN PROGRESS", 24f, middle, new Vector2(0f, -70f),
                       new Vector2(280f, 36f), new Color(1f, 0.6f, 0.1f));
    }

    // Tints only the suit material so skin, visor and shoes keep their colours.
    void ApplyEquippedColour()
    {
        if (player == null)
            return;

        ShopColour colour = EquippedColour();
        bool isDefault = colour == colours[0];
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null || !materials[i].name.StartsWith(suitMaterialName))
                    continue;

                renderer.GetPropertyBlock(block, i);

                if (isDefault)
                {
                    block.Clear();
                }
                else
                {
                    block.SetColor("_BaseColor", colour.color);
                    block.SetColor("_Color", colour.color);
                }

                renderer.SetPropertyBlock(block, i);
            }
        }
    }
}
