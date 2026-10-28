using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > Dodge Game > Set Up Menus
// Builds the "Menus" canvas (start menu, controls, shop, pause, game over / win) in the open scene,
// wires it to MenuManager and saves the scene. Run once; delete "Menus" in the Hierarchy to rebuild.
public static class MenuSetup
{
    static readonly Vector2 Middle = new Vector2(0.5f, 0.5f);
    static readonly Color Backdrop = new Color(0.04f, 0.05f, 0.1f, 0.94f);
    static readonly Color Dim = new Color(0f, 0f, 0f, 0.7f);
    static readonly Color Blue = new Color(0.15f, 0.4f, 0.9f);
    static readonly Color Orange = new Color(1f, 0.55f, 0.05f);
    static readonly Color Grey = new Color(0.25f, 0.27f, 0.33f);
    static readonly Color Title = new Color(1f, 0.6f, 0.1f);
    static readonly Vector2 BigButton = new Vector2(520f, 110f);

    [MenuItem("Tools/Dodge Game/Set Up Menus")]
    public static void SetUpMenus()
    {
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);

        if (gameManager == null)
        {
            EditorUtility.DisplayDialog("Dodge Game", "Open the main scene first (needs GameManager).", "OK");
            return;
        }

        if (GameObject.Find("Menus") != null)
        {
            EditorUtility.DisplayDialog("Dodge Game", "Menus already exist. Delete \"Menus\" in the Hierarchy to rebuild them.", "OK");
            return;
        }

        // Own canvas above GameUI; scales with the screen so it fits every phone.
        GameObject root = new GameObject("Menus", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.layer = 5;
        Undo.RegisterCreatedObjectUndo(root, "Create Menus");

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        MenuManager menu = root.AddComponent<MenuManager>();
        Transform t = root.transform;

        // ---------- Pause button (only shown during a run) ----------
        menu.pauseButton = UIFactory.Button(t, "PauseButton", "II", new Vector2(1f, 1f), new Vector2(-110f, -90f),
                                            new Vector2(130f, 130f), new Color(1f, 1f, 1f, 0.35f), 60f);
        menu.pauseButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

        // ---------- Start menu ----------
        GameObject main = UIFactory.Panel(t, "MainMenuPanel", Backdrop);
        menu.mainMenuPanel = main;
        Header(main.transform, "DODGE", 170f, 330f);
        UIFactory.Text(main.transform, "Subtitle", "Survive the throwers, turrets and the boss", 42f,
                       Middle, new Vector2(0f, 200f), new Vector2(1400f, 70f), new Color(0.8f, 0.85f, 1f));
        menu.playButton = UIFactory.Button(main.transform, "PlayButton", "PLAY", Middle, new Vector2(0f, 60f), BigButton, Orange, 64f);
        menu.shopButton = UIFactory.Button(main.transform, "ShopButton", "SHOP", Middle, new Vector2(0f, -70f), BigButton, Blue);
        menu.controlsButton = UIFactory.Button(main.transform, "ControlsButton", "CONTROLS", Middle, new Vector2(0f, -200f), BigButton, Blue);
        menu.quitButton = UIFactory.Button(main.transform, "QuitButton", "QUIT", Middle, new Vector2(0f, -330f), BigButton, Grey);
        menu.menuOrbsText = UIFactory.Text(main.transform, "OrbsText", "Orbs: 0", 44f, new Vector2(1f, 1f),
                                           new Vector2(-220f, -70f), new Vector2(380f, 70f), new Color(0.3f, 1f, 0.4f));

        // ---------- Controls / How to Play ----------
        GameObject controls = UIFactory.Panel(t, "ControlsPanel", Backdrop);
        menu.controlsPanel = controls;
        Header(controls.transform, "HOW TO PLAY", 100f, 410f);
        TextMeshProUGUI howTo = UIFactory.Text(controls.transform, "Instructions",
            "<color=#FF9A1A>MOVE</color>   Swipe left or right  (A / D or arrow keys)\n\n" +
            "<color=#FF9A1A>BACKFLIP</color>   Tap FLIP to jump over shots  (Space / W / Up)\n\n" +
            "<color=#FF9A1A>ORBS</color>   Grab the green orbs: +25 score and 1 orb for the Shop\n\n" +
            "<color=#FF9A1A>DANGER</color>   Dodge throws, turrets, red circles and boss aim lines\n\n" +
            "<color=#FF9A1A>GOAL</color>   Survive the boss and reach a score of 351 to win\n\n" +
            "<color=#FF9A1A>PAUSE</color>   Pause button (top right) or Esc",
            46f, Middle, new Vector2(0f, 20f), new Vector2(1600f, 620f), Color.white);
        howTo.alignment = TextAlignmentOptions.Left;
        menu.controlsBackButton = UIFactory.Button(controls.transform, "BackButton", "BACK", Middle, new Vector2(0f, -410f), BigButton, Grey);

        // ---------- Shop ----------
        GameObject shop = UIFactory.Panel(t, "ShopPanel", Backdrop);
        menu.shopPanel = shop;
        Header(shop.transform, "SHOP", 110f, 430f);
        menu.shopOrbsText = UIFactory.Text(shop.transform, "OrbsText", "Orbs: 0", 48f, Middle,
                                           new Vector2(0f, 340f), new Vector2(600f, 70f), new Color(0.3f, 1f, 0.4f));

        Image banner = UIFactory.Box(shop.transform, "InProgressBanner", Middle, new Vector2(0f, 265f),
                                     new Vector2(1500f, 64f), new Color(1f, 0.55f, 0.05f, 0.25f));
        UIFactory.Text(banner.transform, "Text", "STILL IN PROGRESS  -  skins arrive when the new models are ready", 34f,
                       Middle, Vector2.zero, new Vector2(1480f, 60f), Title);

        UIFactory.Text(shop.transform, "ColoursHeader", "COLOURS", 40f, Middle, new Vector2(0f, 195f),
                       new Vector2(600f, 60f), Color.white);
        menu.colourGrid = Grid(shop.transform, "ColourGrid", new Vector2(0f, 40f), new Vector2(1500f, 250f), new Vector2(225f, 240f));

        UIFactory.Text(shop.transform, "SkinsHeader", "SKINS", 40f, Middle, new Vector2(0f, -125f),
                       new Vector2(600f, 60f), Color.white);
        menu.skinGrid = Grid(shop.transform, "SkinGrid", new Vector2(0f, -265f), new Vector2(1500f, 210f), new Vector2(300f, 200f));

        menu.shopBackButton = UIFactory.Button(shop.transform, "BackButton", "BACK", Middle, new Vector2(0f, -450f),
                                               new Vector2(400f, 90f), Grey);

        // ---------- Pause ----------
        GameObject pause = UIFactory.Panel(t, "PausePanel", Dim);
        menu.pausePanel = pause;
        Header(pause.transform, "PAUSED", 140f, 250f);
        menu.resumeButton = UIFactory.Button(pause.transform, "ResumeButton", "RESUME", Middle, new Vector2(0f, 60f), BigButton, Orange, 60f);
        menu.pauseRestartButton = UIFactory.Button(pause.transform, "RestartButton", "RESTART", Middle, new Vector2(0f, -70f), BigButton, Blue);
        menu.pauseMainMenuButton = UIFactory.Button(pause.transform, "MainMenuButton", "MAIN MENU", Middle, new Vector2(0f, -200f), BigButton, Grey);

        // ---------- Game Over / Win ----------
        GameObject end = UIFactory.Panel(t, "EndPanel", Dim);
        menu.endPanel = end;
        menu.resultText = Header(end.transform, "GAME OVER", 150f, 280f);
        menu.finalScoreText = UIFactory.Text(end.transform, "FinalScoreText", "Score: 0", 80f, Middle,
                                             new Vector2(0f, 140f), new Vector2(1200f, 110f), Color.white);
        menu.orbsEarnedText = UIFactory.Text(end.transform, "OrbsEarnedText", "Orbs collected: 0", 44f, Middle,
                                             new Vector2(0f, 50f), new Vector2(1400f, 70f), new Color(0.3f, 1f, 0.4f));
        menu.endRestartButton = UIFactory.Button(end.transform, "RestartButton", "RESTART", Middle, new Vector2(0f, -100f), BigButton, Orange, 60f);
        menu.endMainMenuButton = UIFactory.Button(end.transform, "MainMenuButton", "MAIN MENU", Middle, new Vector2(0f, -230f), BigButton, Grey);

        // MenuManager shows the right one at runtime; leave only the start menu visible in the editor.
        controls.SetActive(false);
        shop.SetActive(false);
        pause.SetActive(false);
        end.SetActive(false);
        menu.pauseButton.gameObject.SetActive(false);

        // The old end-of-run UI is replaced by EndPanel; it stays in the scene, switched off.
        if (gameManager.gameOverText != null)
            gameManager.gameOverText.SetActive(false);

        if (gameManager.restartButton != null)
            gameManager.restartButton.SetActive(false);

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
        EditorSceneManager.SaveScene(gameManager.gameObject.scene);

        Debug.Log("Dodge Game: menus created and scene saved.");
    }

    static TextMeshProUGUI Header(Transform parent, string text, float size, float y)
    {
        TextMeshProUGUI header = UIFactory.Text(parent, "Title", text, size, Middle, new Vector2(0f, y),
                                                new Vector2(1600f, size * 1.3f), Title);
        header.fontStyle = FontStyles.Bold;
        return header;
    }

    static RectTransform Grid(Transform parent, string name, Vector2 position, Vector2 size, Vector2 cell)
    {
        RectTransform rect = UIFactory.Rect(parent, name, Middle, position, size);
        GridLayoutGroup grid = rect.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = cell;
        grid.spacing = new Vector2(20f, 20f);
        grid.childAlignment = TextAnchor.MiddleCenter;
        return rect;
    }
}
