using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform muzzle; // Optional: fire point on the model; falls back to above the turret.
    public float fireInterval = 2.4f;
    public float windUpTime = 0.6f;
    public float projectileSpeed = 13f;
    public float turnSpeed = 120f;
    public float muzzleHeight = 1f;
    public Color windUpColor = Color.red;

    private Transform player;
    private float timer;
    private bool stopped = false;
    private Renderer[] renderers;
    private Color[] baseColors;

    void Start()
    {
        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            player = playerMovement.transform;

        renderers = GetComponentsInChildren<Renderer>();
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].material.color;

        // Offset so several turrets don't all fire on the same beat.
        timer = Random.Range(0f, fireInterval * 0.5f);
    }

    void Update()
    {
        if (stopped || player == null || projectilePrefab == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(toPlayer),
                    turnSpeed * Time.deltaTime
                );
        }

        timer += Time.deltaTime;

        // Flash during the wind-up so every shot is announced.
        float windUpStart = fireInterval - windUpTime;

        SetGlow(
            timer >= windUpStart
                ? Mathf.PingPong((timer - windUpStart) * 8f, 1f)
                : 0f
        );

        if (timer >= fireInterval)
        {
            timer = 0f;
            SetGlow(0f);
            Fire();
        }
    }

    void Fire()
    {
        Vector3 firePoint = muzzle != null
            ? muzzle.position
            : transform.position +
              Vector3.up * muzzleHeight +
              transform.forward * 1f;

        Vector3 direction = (player.position - firePoint).normalized;

        GameObject shot =
            Instantiate(projectilePrefab, firePoint, Quaternion.identity);

        Rigidbody rb = shot.GetComponent<Rigidbody>();

        if (rb != null)
            rb.linearVelocity = direction * projectileSpeed;

        Collider shotCollider = shot.GetComponent<Collider>();

        if (shotCollider != null)
        {
            foreach (Collider own in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(own, shotCollider);
        }
    }

    void SetGlow(float amount)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color =
                    Color.Lerp(baseColors[i], windUpColor, amount);
            }
        }
    }

    public void StopFiring()
    {
        stopped = true;
        SetGlow(0f);
    }
}
