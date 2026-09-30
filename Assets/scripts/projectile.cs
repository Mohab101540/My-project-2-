using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Normal,
        Purple,
        Red
    }

    public ProjectileType projectileType = ProjectileType.Normal;

    [Header("Red Projectile")]
    public GameObject normalProjectilePrefab;
    public float splitDelay = 0.6f;
    public float splitSpeed = 12f;

    private bool hasSplit = false;

    void Start()
    {
        if (projectileType == ProjectileType.Red)
        {
            Invoke(nameof(Split), splitDelay);
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

        // Don't split from collision anymore.
        // Red splits automatically after splitDelay.
        Destroy(gameObject);
    }

    void Split()
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

            // Spawn slightly forward so the new projectile
            // isn't inside the old projectile's collider.
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

        // Prevent the 3 new projectiles from immediately
        // destroying each other.
        for (int i = 0; i < spawnedProjectiles.Length; i++)
        {
            Collider colliderA =
                spawnedProjectiles[i].GetComponent<Collider>();

            for (int j = i + 1; j < spawnedProjectiles.Length; j++)
            {
                Collider colliderB =
                    spawnedProjectiles[j].GetComponent<Collider>();

                if (colliderA != null && colliderB != null)
                {
                    Physics.IgnoreCollision(colliderA, colliderB);
                }
            }
        }

        Debug.Log("RED PROJECTILE SPLIT INTO 3!");

        Destroy(gameObject);
    }
}