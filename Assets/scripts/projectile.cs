using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Normal,
        Purple,
        Red,
        Yellow,
        Blue
    }

    public ProjectileType projectileType = ProjectileType.Normal;

    [Header("Red Projectile")]
    public GameObject normalProjectilePrefab;
    public float splitDelay = 0.6f;
    public float splitSpeed = 12f;

    [Header("Yellow Projectile")]
    public float yellowSplitDelay = 0.8f;
    public float yellowSplitSpeed = 10f;
    public float yellowSpreadAngle = 25f;

    private bool hasSplit = false;

    void Start()
    {
        if (projectileType == ProjectileType.Red)
        {
            Invoke(nameof(SplitRed), splitDelay);
        }

        if (projectileType == ProjectileType.Yellow)
        {
            Invoke(nameof(SplitYellow), yellowSplitDelay);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.PlayerDied();
            }
        }

        Destroy(gameObject);
    }

    // =========================
    // RED - SPLITS INTO 3
    // =========================

    void SplitRed()
    {
        if (hasSplit)
            return;

        hasSplit = true;

        Debug.Log("RED PROJECTILE SPLITTING!");

        if (normalProjectilePrefab == null)
        {
            Debug.LogError("RED PROJECTILE: Normal Projectile Prefab is NOT assigned!");
            return;
        }

        Rigidbody originalRb = GetComponent<Rigidbody>();

        Vector3 forwardDirection = Vector3.forward;

        if (originalRb != null && originalRb.linearVelocity.sqrMagnitude > 0.01f)
        {
            forwardDirection = originalRb.linearVelocity.normalized;
        }

        GameObject[] spawnedProjectiles = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            float angle = -30f + (i * 30f);

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * forwardDirection;

            Vector3 spawnPosition =
                transform.position + direction * 0.5f;

            GameObject splitProjectile = Instantiate(
                normalProjectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

            spawnedProjectiles[i] = splitProjectile;

            Rigidbody rb = splitProjectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * splitSpeed;
            }
        }

        IgnoreProjectileCollisions(spawnedProjectiles);

        Debug.Log("RED PROJECTILE SPLIT INTO 3!");

        Destroy(gameObject);
    }

    // =========================
    // YELLOW - SPLITS INTO 5
    // =========================

    void SplitYellow()
    {
        if (hasSplit)
            return;

        hasSplit = true;

        Debug.Log("YELLOW PROJECTILE SPLITTING INTO 5!");

        if (normalProjectilePrefab == null)
        {
            Debug.LogError("YELLOW PROJECTILE: Normal Projectile Prefab is NOT assigned!");
            return;
        }

        Rigidbody originalRb = GetComponent<Rigidbody>();

        Vector3 forwardDirection = Vector3.forward;

        if (originalRb != null && originalRb.linearVelocity.sqrMagnitude > 0.01f)
        {
            forwardDirection = originalRb.linearVelocity.normalized;
        }

        GameObject[] spawnedProjectiles = new GameObject[5];

        for (int i = 0; i < 5; i++)
        {
            float angle = -yellowSpreadAngle * 2f
                        + (i * yellowSpreadAngle);

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * forwardDirection;

            Vector3 spawnPosition =
                transform.position + direction * 0.5f;

            GameObject splitProjectile = Instantiate(
                normalProjectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

            spawnedProjectiles[i] = splitProjectile;

            Rigidbody rb = splitProjectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity =
                    direction.normalized * yellowSplitSpeed;
            }
        }

        IgnoreProjectileCollisions(spawnedProjectiles);

        Debug.Log("YELLOW PROJECTILE SPLIT INTO 5!");

        Destroy(gameObject);
    }

    // =========================
    // IGNORE SPLIT COLLISIONS
    // =========================

    void IgnoreProjectileCollisions(GameObject[] projectiles)
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            Collider colliderA =
                projectiles[i].GetComponent<Collider>();

            for (int j = i + 1; j < projectiles.Length; j++)
            {
                Collider colliderB =
                    projectiles[j].GetComponent<Collider>();

                if (colliderA != null && colliderB != null)
                {
                    Physics.IgnoreCollision(colliderA, colliderB);
                }
            }
        }
    }
}