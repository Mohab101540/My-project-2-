using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > Dodge Game > Set Up Scene
// Adds the gameplay objects to the open scene's Hierarchy (turrets, FLIP button,
// player Rigidbody) and creates the Collectible prefab, then saves the scene.
// Safe to run again: anything that already exists is reused, not duplicated.
public static class DodgeSceneSetup
{
    const string CollectiblePrefabPath = "Assets/prefabs/Collectible.prefab";
    const string CollectibleMaterialPath = "Assets/colors/CollectibleGreen.mat";

    static readonly Vector3 LeftTurretPosition = new Vector3(-5f, 0.03f, 0f);
    static readonly Vector3 RightTurretPosition = new Vector3(5f, 0.03f, 0f);

    [MenuItem("Tools/Dodge Game/Set Up Scene")]
    public static void SetUpScene()
    {
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);

        if (gameManager == null || player == null)
        {
            EditorUtility.DisplayDialog("Dodge Game", "Open the main scene first (needs GameManager and the player).", "OK");
            return;
        }

        SetUpPlayerRigidbody(player);
        SetUpTurrets(gameManager);
        SetUpFlipButton(player);
        SetUpCollectiblePrefab(gameManager);

        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
        EditorSceneManager.SaveScene(gameManager.gameObject.scene);

        Debug.Log("Dodge Game scene set up and saved: " + gameManager.gameObject.scene.path);
    }

    // =========================================================
    // Tools > Dodge Game > Apply New Models
    // =========================================================

    const string PlayerModelPath = "Assets/Models/Remodel_Player.fbx";
    const string ThrowerModelPath = "Assets/Models/Remodel_Thrower.fbx";
    const string BossModelPath = "Assets/Models/Remodel_Boss.fbx";
    const string BossPrefabPath = "Assets/prefabs/BOSS.prefab";

    [MenuItem("Tools/Dodge Game/Apply New Models")]
    public static void ApplyNewModels()
    {
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);

        if (gameManager == null || player == null)
        {
            EditorUtility.DisplayDialog("Dodge Game", "Open the main scene first (needs GameManager and the player).", "OK");
            return;
        }

        // Player: feet on the floor (bottom of its 2 m capsule), facing the throwers (+Z).
        HideOldVisuals(player.gameObject);
        PlaceModel(player.transform, PlayerModelPath, "PlayerModel", 1.8f,
                   player.transform.position.y - 1f, Vector3.forward);

        // Main thrower: on the floor, facing the player (-Z).
        if (gameManager.mainThrower != null)
        {
            GameObject thrower = gameManager.mainThrower.gameObject;
            HideOldVisuals(thrower);
            PlaceModel(thrower.transform, ThrowerModelPath, "ThrowerModel", 1.9f, 0f, Vector3.back);
        }

        // The two throwers beside the boss become black turrets that track the player.
        MakeBlackTurret(gameManager.redThrower, new Color(0.9f, 0.1f, 0.1f), "TurretAccentRed");
        MakeBlackTurret(gameManager.purpleThrower, new Color(0.6f, 0.15f, 0.9f), "TurretAccentPurple");

        ApplyBossModel();

        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
        EditorSceneManager.SaveScene(gameManager.gameObject.scene);

        Debug.Log("Dodge Game: new models applied and scene saved.");
    }

    // Old meshes are switched off, not deleted, so they can be brought back.
    static void HideOldVisuals(GameObject target)
    {
        MeshRenderer ownRenderer = target.GetComponent<MeshRenderer>();

        if (ownRenderer != null)
        {
            Undo.RecordObject(ownRenderer, "Hide old mesh");
            ownRenderer.enabled = false;
        }

        foreach (Transform child in target.transform)
        {
            bool isOurs = child.name == "PlayerModel" || child.name == "ThrowerModel" ||
                          child.name == "BossModel" || child.name.StartsWith("Turret") ||
                          child.name == "throwpoint" || child.name == "BossThrowPoint";

            if (!isOurs && child.GetComponentInChildren<Renderer>(true) != null)
            {
                Undo.RecordObject(child.gameObject, "Hide old model");
                child.gameObject.SetActive(false);
            }
        }
    }

    // Instantiates the model under parent, scales it to targetHeight, stands it on floorY,
    // centres it on the parent and turns it to face faceDirection (world space).
    static GameObject PlaceModel(Transform parent, string modelPath, string name,
                                 float targetHeight, float floorY, Vector3 faceDirection)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
            return existing.gameObject;

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if (asset == null)
        {
            Debug.LogError("Dodge setup: model not found at " + modelPath);
            return null;
        }

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        model.name = name;
        Undo.RegisterCreatedObjectUndo(model, "Add " + name);

        model.transform.rotation = Quaternion.LookRotation(faceDirection);
        model.transform.localPosition = Vector3.zero;

        Bounds bounds = RendererBounds(model);

        if (bounds.size.y > 0.0001f)
            model.transform.localScale *= targetHeight / bounds.size.y;

        bounds = RendererBounds(model);

        Vector3 offset = new Vector3(
            parent.position.x - bounds.center.x,
            floorY - bounds.min.y,
            parent.position.z - bounds.center.z);

        model.transform.position += offset;

        return model;
    }

    static Bounds RendererBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(root.transform.position, Vector3.zero);

        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }

    static void MakeBlackTurret(GameObject thrower, Color accent, string accentMaterialName)
    {
        if (thrower == null || thrower.transform.Find("TurretBody") != null)
            return;

        Material black = GetOrCreateMaterial("TurretBlack", new Color(0.07f, 0.07f, 0.08f), 0.6f);
        Material barrelMaterial = GetOrCreateMaterial("TurretBarrel", new Color(0.22f, 0.22f, 0.25f), 0.7f);
        Material accentMaterial = GetOrCreateMaterial(accentMaterialName, accent, 0.2f);

        HideOldVisuals(thrower);

        GameObject body = new GameObject("TurretBody");
        Undo.RegisterCreatedObjectUndo(body, "Add black turret");
        body.transform.SetParent(thrower.transform, false);

        AddPart(body.transform, PrimitiveType.Cylinder, "Base", black,
                new Vector3(0f, 0.5f, 0f), new Vector3(1.2f, 0.5f, 1.2f), Vector3.zero);
        AddPart(body.transform, PrimitiveType.Cube, "Barrel", barrelMaterial,
                new Vector3(0f, 1f, 0.6f), new Vector3(0.3f, 0.3f, 1.2f), Vector3.zero);
        AddPart(body.transform, PrimitiveType.Cylinder, "BarrelRing", accentMaterial,
                new Vector3(0f, 1f, 1.15f), new Vector3(0.42f, 0.05f, 0.42f), new Vector3(90f, 0f, 0f));

        Thrower throwerComponent = thrower.GetComponent<Thrower>();

        if (throwerComponent != null)
        {
            Undo.RecordObject(throwerComponent, "Turn to player");
            throwerComponent.turnToPlayer = true;
            EditorUtility.SetDirty(throwerComponent);
        }
    }

    static void AddPart(Transform parent, PrimitiveType shape, string name, Material material,
                        Vector3 localPosition, Vector3 localScale, Vector3 localEuler)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        part.name = name;
        Object.DestroyImmediate(part.GetComponent<Collider>()); // the thrower keeps its own collider
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.transform.localEulerAngles = localEuler;
        part.GetComponent<Renderer>().sharedMaterial = material;
    }

    static Material GetOrCreateMaterial(string name, Color color, float metallic)
    {
        string path = "Assets/colors/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", 0.5f);
            AssetDatabase.CreateAsset(material, path);
        }

        return material;
    }

    static void ApplyBossModel()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(BossPrefabPath);

        try
        {
            if (contents.transform.Find("BossModel") == null)
            {
                HideOldVisuals(contents);

                // Boss spawns with its root at y = 2 (GameManager default spawn point); stand it on the floor.
                PlaceModel(contents.transform, BossModelPath, "BossModel", 3.4f,
                           contents.transform.position.y - 2f, Vector3.back);

                PrefabUtility.SaveAsPrefabAsset(contents, BossPrefabPath);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    // =========================================================
    // Tools > Dodge Game > Speed Up Editor
    // =========================================================

    [MenuItem("Tools/Dodge Game/Speed Up Editor")]
    public static void SpeedUpEditor()
    {
        // Skips the ~2-3 s script reload every time Play is pressed.
        // Safe here: the game scripts keep no static state between runs.
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        // DirectX 11 is usually smoother than DirectX 12 for the Windows editor; takes effect after restarting Unity.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,
            new[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11, UnityEngine.Rendering.GraphicsDeviceType.Direct3D12 });

        AssetDatabase.SaveAssets();

        Debug.Log("Dodge Game: Play starts without a script reload now. Restart Unity to switch the editor to DirectX 11.");
    }

    static void SetUpPlayerRigidbody(PlayerMovement player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb == null)
            rb = Undo.AddComponent<Rigidbody>(player.gameObject);

        Undo.RecordObject(rb, "Set up player Rigidbody");
        rb.mass = 1f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    static void SetUpTurrets(GameManager gameManager)
    {
        GameObject projectile =
            gameManager.mainThrower != null ? gameManager.mainThrower.normalProjectile : null;

        Turret left = GetOrCreateTurret("TurretLeft", gameManager.redThrower, LeftTurretPosition, projectile);
        Turret right = GetOrCreateTurret("TurretRight", gameManager.purpleThrower, RightTurretPosition, projectile);

        Undo.RecordObject(gameManager, "Assign turrets");
        gameManager.turrets = new[] { left, right };
        EditorUtility.SetDirty(gameManager);
    }

    static Turret GetOrCreateTurret(string name, GameObject model, Vector3 position, GameObject projectile)
    {
        GameObject existing = GameObject.Find(name);
        GameObject turretObject;

        if (existing != null)
        {
            turretObject = existing;
        }
        else
        {
            if (model == null)
            {
                Debug.LogError("Dodge setup: no thrower model for " + name);
                return null;
            }

            // A copy of the red/purple thrower: same mesh and colour, its own object.
            turretObject = Object.Instantiate(model, position, model.transform.rotation);
            turretObject.name = name;
            turretObject.SetActive(true);
            Undo.RegisterCreatedObjectUndo(turretObject, "Create " + name);

            Thrower thrower = turretObject.GetComponent<Thrower>();

            if (thrower != null)
                Undo.DestroyObjectImmediate(thrower);
        }

        Turret turret = turretObject.GetComponent<Turret>();

        if (turret == null)
            turret = Undo.AddComponent<Turret>(turretObject);

        Undo.RecordObject(turret, "Set up " + name);
        turret.projectilePrefab = projectile;

        Transform throwPoint = turretObject.transform.Find("throwpoint");

        if (throwPoint != null)
            turret.muzzle = throwPoint;

        // Waits idle until GameManager switches it on at the unlock score.
        turret.enabled = false;
        EditorUtility.SetDirty(turret);

        return turret;
    }

    static void SetUpFlipButton(PlayerMovement player)
    {
        if (player.jumpButton != null)
            return;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

        if (canvas == null)
        {
            Debug.LogError("Dodge setup: no UI Canvas found for the FLIP button.");
            return;
        }

        GameObject buttonObject =
            new GameObject("FlipButton", typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(buttonObject, "Create FlipButton");
        buttonObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-40f, 40f);
        rect.sizeDelta = new Vector2(220f, 220f);

        buttonObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.6f);

        GameObject label = new GameObject("Label", typeof(RectTransform));
        label.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
        text.text = "FLIP";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 60f;
        text.color = Color.black;

        Undo.RecordObject(player, "Assign FLIP button");
        player.jumpButton = buttonObject.GetComponent<Button>();
        EditorUtility.SetDirty(player);
    }

    static void SetUpCollectiblePrefab(GameManager gameManager)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CollectiblePrefabPath);

        if (prefab == null)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CollectibleMaterialPath);

            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.2f, 1f, 0.3f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 0.2f));
                AssetDatabase.CreateAsset(material, CollectibleMaterialPath);
            }

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Collectible";
            orb.transform.localScale = Vector3.one * 0.6f;
            orb.GetComponent<Collider>().isTrigger = true;
            orb.GetComponent<Renderer>().sharedMaterial = material;
            orb.AddComponent<Collectible>();

            prefab = PrefabUtility.SaveAsPrefabAsset(orb, CollectiblePrefabPath);
            Object.DestroyImmediate(orb);
        }

        Undo.RecordObject(gameManager, "Assign Collectible prefab");
        gameManager.collectiblePrefab = prefab;
        EditorUtility.SetDirty(gameManager);
    }
}
