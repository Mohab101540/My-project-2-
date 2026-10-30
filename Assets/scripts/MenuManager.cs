using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Every screen outside the arena: start menu, controls, shop, pause, game over / win.
// Lives on the "Menus" canvas in the main scene. Every button calls these public methods
// from its OnClick() list in the Inspector.
// Restart reloads straight into a run, Main Menu reloads to the menu.
public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopColour
    {
        public string id;
        public string displayName;
        public int price;
        public Color color;
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
    public TextMeshProUGUI menuOrbsText;

    [Header("Game Over / Win")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI orbsEarnedText;

    [Header("Shop")]
    public TextMeshProUGUI shopOrbsText;
    public RectTransform colourGrid;
    public ShopCard cardPrefab; // Assets/prefabs/ShopCard
    [Tooltip("Colours for sale. The first is the default: free, and leaves the model's own colours untouched.")]
    public ShopColour[] colours;
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

    public void ShowMainMenu()
    {
        state = State.MainMenu;
        Freeze(true);
        menuOrbsText.text = "Orbs: " + ShopData.Orbs;
        ShowOnly(mainMenuPanel);
    }

    public void ShowControls()
    {
        ShowOnly(controlsPanel);
    }

    public void StartRun()
    {
        state = State.Playing;
        ApplyEquippedColour();
        Freeze(false);
        ShowOnly(null);
    }

    public void Pause()
    {
        if (state != State.Playing)
            return;

        state = State.Paused;
        Freeze(true);
        ShowOnly(pausePanel);
    }

    public void Resume()
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

    public void Restart()
    {
        skipMenuOnLoad = true;
        gameManager.RestartGame();
    }

    public void MainMenu()
    {
        skipMenuOnLoad = false;
        gameManager.RestartGame();
    }

    public void Quit()
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

    public void OpenShop()
    {
        RefreshShop();
        ShowOnly(shopPanel);
    }

    ShopColour EquippedColour()
    {
        if (colours.Length == 0)
            return null;

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
        ShopColour equippedColour = EquippedColour();

        // Hidden first so the grid lays out the new cards this frame, not next to the old ones.
        foreach (Transform child in colourGrid)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        for (int i = 0; i < colours.Length; i++)
            AddColourCard(i, colours[i] == equippedColour);
    }

    void AddColourCard(int index, bool equipped)
    {
        ShopColour colour = colours[index];
        ShopCard card = Instantiate(cardPrefab, colourGrid);
        card.name = colour.displayName;
        card.menu = this;
        card.colourIndex = index;
        card.swatch.color = colour.color;
        card.nameText.text = colour.displayName;

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

        card.buttonLabel.text = label;
        card.button.image.color = buttonColour;
        card.button.interactable = !equipped;
    }

    // Called by ShopCard when its button is pressed.
    public void BuyOrEquip(int index)
    {
        ShopColour colour = colours[index];

        if (ShopData.IsOwned(colour.id, colour.price))
            ShopData.Equip(colour.id);
        else if (ShopData.TryBuy(colour.id, colour.price))
            ShopData.Equip(colour.id);

        RefreshShop();
    }

    // Tints only the suit material so skin, visor and shoes keep their colours.
    void ApplyEquippedColour()
    {
        ShopColour colour = EquippedColour();

        if (player == null || colour == null)
            return;

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
