using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Normal,
        Purple,
        Red,
        Yellow,
        Blue,
        Bouncer,
        Explosive
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

    [Header("Bouncer Settings")]
    public float bouncerDirectionChange = 25f;
    public float bouncerMinSpeed = 7f;
    public float bouncerMaxSpeed = 12f;
    public int bouncerMaxBounces = 8;

    [Header("Explosive")]
    public float explosionRadius = 1.8f;
    public Color explosiveColor = new Color(1f, 0.45f, 0f);

    // Set by the thrower before Start: where the lob lands.
    [HideInInspector] public Vector3 explosiveTarget;

    [Header("Cleanup")]
    public float maxLifetime = 10f;

    private bool hasSplit = false;
    private int bounceCount = 0;
    private bool exploded = false;
    private GameObject warningRing;

    void Start()
    {
        // Safety net for projectiles that escape the arena without hitting anything.
        Destroy(gameObject, maxLifetime);

        // Fast projectiles (purple ~28 m/s) can skip past the player with discrete collision.
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if (projectileType == ProjectileType.Red)
        {
            Invoke(nameof(SplitRed), splitDelay);
        }

        if (projectileType == ProjectileType.Yellow)
        {
            Invoke(nameof(SplitYellow), yellowSplitDelay);
        }

        if (projectileType == ProjectileType.Explosive)
        {
            SetUpExplosive();
        }
    }

    private void OnDestroy()
    {
        if (warningRing != null)
            Destroy(warningRing);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // =========================
        // PLAYER HIT
        // =========================

        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager gameManager =
                FindFirstObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.PlayerDied();
            }

            Destroy(gameObject);
            return;
        }

        // =========================
        // EXPLOSIVE
        // =========================

        if (projectileType == ProjectileType.Explosive)
        {
            Explode();
            return;
        }

        // =========================
        // BOUNCER
        // =========================

        if (projectileType == ProjectileType.Bouncer)
        {
            HandleBouncerBounce(collision);
            return;
        }

        // =========================
        // OTHER PROJECTILES
        // =========================

        Destroy(gameObject);
    }

    // =========================
    // EXPLOSIVE - WARNING + BLAST
    // =========================

    void SetUpExplosive()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.useGravity = true;

        transform.localScale *= 1.4f;

        Renderer projectileRenderer = GetComponent<Renderer>();

        if (projectileRenderer != null)
            projectileRenderer.material.color = explosiveColor;

        // Flat red disc on the floor where the lob will land.
        warningRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warningRing.name = "ExplosiveWarning";
        Destroy(warningRing.GetComponent<Collider>());

        warningRing.transform.position =
            new Vector3(explosiveTarget.x, 0.02f, explosiveTarget.z);
        warningRing.transform.localScale =
            new Vector3(explosionRadius * 2f, 0.01f, explosionRadius * 2f);
        warningRing.GetComponent<Renderer>().material.color =
            new Color(0.8f, 0f, 0f);
    }

    void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        Vector3 center = transform.position;

        foreach (Collider hit in Physics.OverlapSphere(center, explosionRadius))
        {
            if (hit.CompareTag("Player"))
            {
                GameManager gameManager =
                    FindFirstObjectByType<GameManager>();

                if (gameManager != null)
                    gameManager.PlayerDied();

                break;
            }
        }

        GameObject hazard = new GameObject("Explosion");
        hazard.transform.position = new Vector3(center.x, 0.03f, center.z);

        ExplosionHazard explosion = hazard.AddComponent<ExplosionHazard>();
        explosion.blastRadius = explosionRadius;

        Destroy(gameObject);
    }

    // =========================
    // BOUNCER MOVEMENT
    // =========================

    void HandleBouncerBounce(Collision collision)
    {
        bounceCount++;

        if (bounceCount >= bouncerMaxBounces)
        {
            Destroy(gameObject);
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
            return;

        // Get the direction the projectile was already moving.
        Vector3 currentDirection = rb.linearVelocity.normalized;

        // Add a random horizontal direction change.
        float randomAngle =
            Random.Range(
                -bouncerDirectionChange,
                bouncerDirectionChange
            );

        Vector3 newDirection =
            Quaternion.Euler(
                0f,
                randomAngle,
                0f
            ) * currentDirection;

        // Add a small random speed change.
        float newSpeed =
            Random.Range(
                bouncerMinSpeed,
                bouncerMaxSpeed
            );

        // Keep the physics bounce's vertical direction,
        // but make the horizontal movement more unpredictable.
        Vector3 velocity = rb.linearVelocity;

        Vector3 horizontalDirection =
            new Vector3(
                newDirection.x,
                0f,
                newDirection.z
            ).normalized;

        float verticalSpeed = velocity.y;

        rb.linearVelocity =
            horizontalDirection * newSpeed;

        // Preserve some of the bounce's vertical movement.
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            verticalSpeed,
            rb.linearVelocity.z
        );
    }

    // =========================
    // RED - SPLITS INTO 3
    // =========================

    void SplitRed()
    {
        if (hasSplit)
            return;

        hasSplit = true;

        if (normalProjectilePrefab == null)
        {
            Debug.LogError(
                "RED PROJECTILE: Normal Projectile Prefab is NOT assigned!"
            );

            return;
        }

        Rigidbody originalRb = GetComponent<Rigidbody>();

        Vector3 forwardDirection = Vector3.forward;

        if (originalRb != null &&
            originalRb.linearVelocity.sqrMagnitude > 0.01f)
        {
            forwardDirection =
                originalRb.linearVelocity.normalized;
        }

        GameObject[] spawnedProjectiles =
            new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            float angle =
                -30f + (i * 30f);

            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) * forwardDirection;

            Vector3 spawnPosition =
                transform.position +
                direction * 0.5f;

            GameObject splitProjectile =
                Instantiate(
                    normalProjectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            spawnedProjectiles[i] =
                splitProjectile;

            Rigidbody rb =
                splitProjectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity =
                    direction.normalized *
                    splitSpeed;
            }
        }

        IgnoreProjectileCollisions(
            spawnedProjectiles
        );

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

        if (normalProjectilePrefab == null)
        {
            Debug.LogError(
                "YELLOW PROJECTILE: Normal Projectile Prefab is NOT assigned!"
            );

            return;
        }

        Rigidbody originalRb =
            GetComponent<Rigidbody>();

        Vector3 forwardDirection =
            Vector3.forward;

        if (originalRb != null &&
            originalRb.linearVelocity.sqrMagnitude > 0.01f)
        {
            forwardDirection =
                originalRb.linearVelocity.normalized;
        }

        GameObject[] spawnedProjectiles =
            new GameObject[5];

        for (int i = 0; i < 5; i++)
        {
            float angle =
                -yellowSpreadAngle * 2f +
                (i * yellowSpreadAngle);

            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) * forwardDirection;

            Vector3 spawnPosition =
                transform.position +
                direction * 0.5f;

            GameObject splitProjectile =
                Instantiate(
                    normalProjectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            spawnedProjectiles[i] =
                splitProjectile;

            Rigidbody rb =
                splitProjectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity =
                    direction.normalized *
                    yellowSplitSpeed;
            }
        }

        IgnoreProjectileCollisions(
            spawnedProjectiles
        );

        Destroy(gameObject);
    }

    // =========================
    // IGNORE SPLIT COLLISIONS
    // =========================

    void IgnoreProjectileCollisions(
        GameObject[] projectiles
    )
    {
        for (int i = 0;
             i < projectiles.Length;
             i++)
        {
            Collider colliderA =
                projectiles[i]
                .GetComponent<Collider>();

            for (int j = i + 1;
                 j < projectiles.Length;
                 j++)
            {
                Collider colliderB =
                    projectiles[j]
                    .GetComponent<Collider>();

                if (colliderA != null &&
                    colliderB != null)
                {
                    Physics.IgnoreCollision(
                        colliderA,
                        colliderB
                    );
                }
            }
        }
    }
}