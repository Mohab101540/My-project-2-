using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Over")]
    public GameObject gameOverText;
    public GameObject restartButton;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public float scorePerSecond = 10f;

    [Header("Main Thrower")]
    public Thrower mainThrower;

    [Header("Extra Throwers")]
    public GameObject redThrower;
    public GameObject purpleThrower;

    [Header("Boss")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Milestone UI")]
    public GameObject milestoneText;

    [Header("Collectibles")]
    public float collectibleInterval = 4f;
    public float collectibleValue = 25f;
    public GameObject collectiblePrefab; // Assets/prefabs/Collectible

    [Header("Progression (score)")]
    public float blueUnlockScore = 75f;
    public float bouncerUnlockScore = 125f;
    public float explosiveUnlockScore = 150f;
    public float bossScore = 201f;
    public float bossPhaseTwoScore = 276f;
    public float winScore = 351f;

    [Header("Turrets")]
    // The two Turret objects in the Hierarchy. Their Turret component starts disabled
    // and is switched on at turretUnlockScore.
    public Turret[] turrets;
    public float turretUnlockScore = 50f;

    private float score;
    private bool turretsSpawned = false;
    private PlayerMovement playerMovement;
    private float collectibleTimer = 0f;
    private bool gameOver = false;

    private bool purpleUnlocked = false;
    private bool redUnlocked = false;

    private bool extraThrowersActive = false;
    private bool extraThrowersDone = false;

    private bool bossActive = false;
    private bool bossPhaseTwo = false;
    private GameObject currentBoss;

    void Start()
    {
        score = 0f;

        // The previous run froze time on game over / win.
        Time.timeScale = 1f;

        playerMovement = FindFirstObjectByType<PlayerMovement>();

        UpdateScoreUI();

        // Extra throwers start disabled.
        if (redThrower != null)
            redThrower.SetActive(false);

        if (purpleThrower != null)
            purpleThrower.SetActive(false);

        // Boss starts disabled because we spawn it later.
        bossActive = false;
        currentBoss = null;

        // Hide Game Over UI.
        if (gameOverText != null)
            gameOverText.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(false);

        if (milestoneText != null)
            milestoneText.SetActive(false);
    }

    void Update()
    {
        if (gameOver)
            return;

        // Increase score.
        score += scorePerSecond * Time.deltaTime;

        UpdateScoreUI();

        // =========================================
        // COLLECTIBLES
        // =========================================

        collectibleTimer += Time.deltaTime;

        if (collectibleTimer >= collectibleInterval)
        {
            collectibleTimer = 0f;
            SpawnCollectible();
        }

        // =========================================
        // MAIN THROWER DIFFICULTY
        // =========================================

        if (mainThrower != null)
        {
            int difficultyLevel =
                Mathf.FloorToInt(score / 100f);

            float newInterval =
                Mathf.Max(
                    0.7f,
                    2.5f - difficultyLevel * 0.5f
                );

            mainThrower.throwInterval =
                newInterval;

            // Purple unlock at 50.
            if (score >= 50f && !purpleUnlocked)
            {
                purpleUnlocked = true;

                mainThrower.UnlockPurple();

                Debug.Log(
                    "PURPLE PROJECTILE UNLOCKED!"
                );
            }

            // Red unlock at 100.
            if (score >= 100f && !redUnlocked)
            {
                redUnlocked = true;

                mainThrower.UnlockRed();

                Debug.Log(
                    "RED PROJECTILE UNLOCKED!"
                );
            }

            if (score >= blueUnlockScore && !mainThrower.blueUnlocked)
            {
                mainThrower.UnlockBlue();
            }

            if (score >= bouncerUnlockScore && !mainThrower.bouncerUnlocked)
            {
                mainThrower.UnlockBouncer();
                ShowMilestone("BOUNCERS!");
            }

            if (score >= explosiveUnlockScore && !mainThrower.explosiveUnlocked)
            {
                mainThrower.UnlockYellow();
                mainThrower.UnlockExplosive();
                ShowMilestone("EXPLOSIVES! WATCH THE RED CIRCLES");
            }
        }

        // =========================================
        // TURRETS
        // =========================================

        if (score >= turretUnlockScore && !turretsSpawned && !bossActive)
        {
            StartTurrets();
        }

        // =========================================
        // BOSS: SCORE 100
        // =========================================

        if (score >= bossScore && !bossActive)
        {
            // The boss fight runs alone so its patterns stay readable.
            StopTurrets();
            SpawnBoss();
        }

        if (score >= bossPhaseTwoScore && bossActive && !bossPhaseTwo && currentBoss != null)
        {
            bossPhaseTwo = true;

            BossController boss =
                currentBoss.GetComponent<BossController>();

            if (boss != null)
                boss.EnterPhaseTwo();

            ShowMilestone("BOSS ENRAGED!");
        }

        // =========================================
        // BOSS: SCORE 200
        // =========================================

        if (score >= winScore && bossActive)
        {
            DestroyBoss();
            WinGame();
            return;
        }

        // =========================================
        // EXTRA THROWERS: SCORE 1000
        // =========================================

        // extraThrowersDone stops this from re-enabling them every frame after 200.
        if (score >= 100f && !extraThrowersActive && !extraThrowersDone)
        {
            extraThrowersActive = true;

            if (redThrower != null)
                redThrower.SetActive(true);

            if (purpleThrower != null)
                purpleThrower.SetActive(true);

            ShowMilestone(
                "DOUBLE THROWERS!"
            );

            Debug.Log(
                "EXTRA THROWERS ACTIVE!"
            );
        }

        // =========================================
        // EXTRA THROWERS: SCORE 1200
        // =========================================

        if (score >= 200f && extraThrowersActive)
        {
            extraThrowersActive = false;
            extraThrowersDone = true;

            if (redThrower != null)
                redThrower.SetActive(false);

            if (purpleThrower != null)
                purpleThrower.SetActive(false);

            ShowMilestone(
                "TRIAL COMPLETE!"
            );

            Debug.Log(
                "EXTRA THROWERS REMOVED!"
            );
        }
    }

    // =============================================
    // BOSS SPAWN
    // =============================================

    void SpawnBoss()
    {
        bossActive = true;

        if (bossPrefab == null)
        {
            Debug.LogError(
                "BOSS PREFAB IS NOT ASSIGNED!"
            );

            return;
        }

        Vector3 spawnPosition;

        if (bossSpawnPoint != null)
        {
            spawnPosition =
                bossSpawnPoint.position;
        }
        else
        {
            // Default Boss position.
            spawnPosition =
                new Vector3(
                    0f,
                    2f,
                    8f
                );
        }

        currentBoss =
            Instantiate(
                bossPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Debug.Log(
            "BOSS HAS SPAWNED!"
        );

        ShowMilestone(
            "BOSS BATTLE!"
        );
    }

    // =============================================
    // BOSS DESTROY
    // =============================================

    void DestroyBoss()
    {
        bossActive = false;

        if (currentBoss != null)
        {
            BossController boss =
                currentBoss.GetComponent<BossController>();

            if (boss != null)
            {
                boss.StopAttacking();
            }

            Destroy(currentBoss);

            currentBoss = null;
        }

        Debug.Log(
            "BOSS HAS BEEN DEFEATED!"
        );

        ShowMilestone(
            "BOSS DEFEATED!"
        );
    }

    // =============================================
    // SCORE UI
    // =============================================

    public void AddScore(float amount)
    {
        if (gameOver)
            return;

        score += amount;

        UpdateScoreUI();
    }

    // =============================================
    // TURRETS
    // =============================================

    void StartTurrets()
    {
        turretsSpawned = true;

        foreach (Turret turret in turrets)
        {
            if (turret != null)
                turret.enabled = true;
        }

        ShowMilestone("TURRETS ONLINE!");
    }

    void StopTurrets()
    {
        foreach (Turret turret in turrets)
        {
            if (turret != null)
                turret.StopFiring();
        }
    }

    // =============================================
    // COLLECTIBLE SPAWN
    // =============================================

    void SpawnCollectible()
    {
        if (playerMovement == null || collectiblePrefab == null)
            return;

        Transform player = playerMovement.transform;

        // Spawn on the player's movement line so every orb is reachable.
        Vector3 spawnPosition =
            new Vector3(
                Random.Range(
                    -playerMovement.arenaLimit,
                    playerMovement.arenaLimit
                ),
                player.position.y,
                player.position.z
            );

        GameObject orb =
            Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);

        Collectible collectible = orb.GetComponent<Collectible>();

        if (collectible != null)
            collectible.value = collectibleValue;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                Mathf.FloorToInt(score).ToString();
        }
    }

    // =============================================
    // PLAYER DEATH
    // =============================================

    public void PlayerDied()
    {
        if (gameOver)
            return;

        Debug.Log(
            "GAME OVER! Score: " +
            Mathf.FloorToInt(score)
        );

        EndGame("GAME OVER");
    }

    // =============================================
    // WIN: SURVIVE THE BOSS
    // =============================================

    void WinGame()
    {
        if (gameOver)
            return;

        Debug.Log(
            "YOU WIN! Score: " +
            Mathf.FloorToInt(score)
        );

        EndGame("YOU WIN!");
    }

    void EndGame(string message)
    {
        gameOver = true;

        // Stop ALL throwers.
        Thrower[] throwers =
            FindObjectsByType<Thrower>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Thrower thrower in throwers)
        {
            thrower.StopThrowing();
        }

        // Stop Boss.
        if (currentBoss != null)
        {
            BossController boss =
                currentBoss.GetComponent<BossController>();

            if (boss != null)
            {
                boss.StopAttacking();
            }
        }

        StopTurrets();

        CancelInvoke(nameof(HideMilestone));
        HideMilestone();

        // Show Game Over / Win.
        if (gameOverText != null)
        {
            TextMeshProUGUI text =
                gameOverText.GetComponentInChildren<TextMeshProUGUI>(true);

            if (text != null)
            {
                text.text =
                    message + "\nScore: " +
                    Mathf.FloorToInt(score);
            }

            gameOverText.SetActive(true);
        }

        // Show Restart button.
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }

        // Freezes projectiles, splits, collectibles and the player until restart.
        Time.timeScale = 0f;
    }

    // =============================================
    // RESTART
    // =============================================

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =============================================
    // MILESTONE POPUP
    // =============================================

    void ShowMilestone(string message)
    {
        if (milestoneText == null)
            return;

        TextMeshProUGUI text =
            milestoneText.GetComponent<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = message;

            milestoneText.SetActive(true);

            CancelInvoke(
                nameof(HideMilestone)
            );

            Invoke(
                nameof(HideMilestone),
                2f
            );
        }
    }

    void HideMilestone()
    {
        if (milestoneText != null)
        {
            milestoneText.SetActive(false);
        }
    }
}