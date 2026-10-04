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

    private float score;
    private bool gameOver = false;

    private bool purpleUnlocked = false;
    private bool redUnlocked = false;

    private bool extraThrowersActive = false;

    private bool bossActive = false;
    private GameObject currentBoss;

    void Start()
    {
        score = 0f;

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
        }

        // =========================================
        // BOSS: SCORE 100
        // =========================================

        if (score >= 201f && !bossActive)
        {
            SpawnBoss();
        }

        // =========================================
        // BOSS: SCORE 200
        // =========================================

        if (score >= 301f && bossActive)
        {
            DestroyBoss();
        }

        // =========================================
        // EXTRA THROWERS: SCORE 1000
        // =========================================

        if (score >= 100f && !extraThrowersActive)
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

        gameOver = true;

        Debug.Log(
            "GAME OVER! Score: " +
            Mathf.FloorToInt(score)
        );

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

        // Show Game Over.
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }

        // Show Restart button.
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

    // =============================================
    // RESTART
    // =============================================

    public void RestartGame()
    {
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