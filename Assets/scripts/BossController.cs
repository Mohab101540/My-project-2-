using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform throwPoint;
    public GameObject projectilePrefab;

    [Header("General")]
    public float attackInterval = 2.5f;
    public float projectileSpeed = 16f;
    public float spawnOffset = 1f;

    [Header("Telegraph")]
    public float telegraphTime = 0.7f;
    public Color telegraphColor = new Color(1f, 0.5f, 0f);
    public float aimLineLength = 18f;
    public LineRenderer[] aimLines; // The AimLine children of the BOSS prefab, one per shot in a spread.

    [Header("Phase 2")]
    public float phaseTwoIntervalMultiplier = 0.6f;
    public float phaseTwoSpeedMultiplier = 1.2f;
    public float phaseTwoTelegraphMultiplier = 0.7f;

    private static readonly float[] SpreadAngles = { -50f, -25f, 0f, 25f, 50f };
    private static readonly float[] BurstAngles = { 0f, -45f, 45f, -90f, 90f };
    private static readonly float[] SingleAngle = { 0f };

    private int attackPattern = 0;
    private bool attacking = false;
    private bool stopped = false;
    private bool phaseTwo = false;
    private float attackTimer = 0f;

    private Renderer[] renderers;
    private Color[] baseColors;

    void Start()
    {
        attackTimer = attackInterval;

        renderers = GetComponentsInChildren<MeshRenderer>();
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].material.color;

        // Automatically find the Player when the Boss spawns.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("BOSS: Could not find Player!");
            }
        }
    }

    void Update()
    {
        if (stopped)
            return;

        if (player == null || projectilePrefab == null)
            return;

        if (attacking)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            StartCoroutine(NextAttack());
        }
    }

    IEnumerator NextAttack()
    {
        attacking = true;

        // Aim is locked when the telegraph starts, so the lines show exactly where shots go.
        Vector3 aim = GetPlayerDirection();

        float[] telegraphAngles =
            attackPattern == 0 || attackPattern == 2 ? SpreadAngles
            : attackPattern == 3 ? BurstAngles
            : SingleAngle;

        yield return Telegraph(aim, telegraphAngles);

        switch (attackPattern)
        {
            case 0:
                SpreadAttack(aim);
                break;

            case 1:
                yield return RapidAttack();
                break;

            case 2:
                SpreadAttack(aim);

                yield return new WaitForSeconds(0.2f);

                SpreadAttack(aim);
                break;

            case 3:
                BurstAttack(aim);
                break;

            case 4:
                yield return DeathBarrage();
                break;
        }

        attackPattern++;

        if (attackPattern >= 5)
            attackPattern = 0;

        attacking = false;
    }

    // =========================
    // TELEGRAPH
    // =========================

    IEnumerator Telegraph(Vector3 aim, float[] angles)
    {
        ShowAimLines(aim, angles);

        float elapsed = 0f;

        while (elapsed < telegraphTime)
        {
            elapsed += Time.deltaTime;
            SetGlow(Mathf.PingPong(elapsed * 6f, 1f));
            yield return null;
        }

        SetGlow(0f);
        HideAimLines();
    }

    void ShowAimLines(Vector3 aim, float[] angles)
    {
        // Drawn on the floor so the top-down camera reads them clearly.
        Vector3 start = throwPoint.position;
        start.y = 0.05f;

        for (int i = 0; i < angles.Length; i++)
        {
            LineRenderer line = aimLines[i];
            Vector3 direction = Quaternion.Euler(0f, angles[i], 0f) * aim;

            line.SetPosition(0, start);
            line.SetPosition(1, start + direction * aimLineLength);
            line.enabled = true;
        }
    }

    void HideAimLines()
    {
        foreach (LineRenderer line in aimLines)
            line.enabled = false;
    }

    void SetGlow(float amount)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color =
                    Color.Lerp(baseColors[i], telegraphColor, amount);
            }
        }
    }

    // =========================
    // PHASE 2
    // =========================

    public void EnterPhaseTwo()
    {
        if (phaseTwo)
            return;

        phaseTwo = true;

        attackInterval *= phaseTwoIntervalMultiplier;
        projectileSpeed *= phaseTwoSpeedMultiplier;
        telegraphTime *= phaseTwoTelegraphMultiplier;

        for (int i = 0; i < baseColors.Length; i++)
            baseColors[i] = Color.Lerp(baseColors[i], Color.red, 0.6f);

        SetGlow(0f);
    }

    // =========================
    // ATTACKS
    // =========================

    void SpreadAttack(Vector3 direction)
    {
        foreach (float angle in SpreadAngles)
        {
            Vector3 newDirection =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) * direction;

            FireProjectile(newDirection);
        }
    }

    IEnumerator RapidAttack()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 direction = GetPlayerDirection();

            direction =
                Quaternion.Euler(
                    0f,
                    Random.Range(-15f, 15f),
                    0f
                ) * direction;

            FireProjectile(direction);

            yield return new WaitForSeconds(0.15f);
        }
    }

    void BurstAttack(Vector3 playerDirection)
    {
        foreach (float angle in BurstAngles)
        {
            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) * playerDirection;

            FireProjectile(direction);
        }
    }

    IEnumerator DeathBarrage()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 direction =
                GetPlayerDirection();

            float spread =
                Mathf.Lerp(
                    0f,
                    40f,
                    (float)i / 14f
                );

            direction =
                Quaternion.Euler(
                    0f,
                    Random.Range(-spread, spread),
                    0f
                ) * direction;

            FireProjectile(direction);

            yield return new WaitForSeconds(0.1f);
        }
    }

    void FireProjectile(Vector3 direction)
    {
        direction.Normalize();

        Vector3 spawnPosition =
            throwPoint.position +
            direction * spawnOffset;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

        Rigidbody rb =
            projectile.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(
                "BOSS PROJECTILE HAS NO RIGIDBODY!"
            );

            return;
        }

        rb.linearVelocity =
            direction * projectileSpeed;
    }

    Vector3 GetPlayerDirection()
    {
        Vector3 direction =
            player.position -
            throwPoint.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return transform.forward;

        return direction.normalized;
    }

    public void StopAttacking()
    {
        stopped = true;
        StopAllCoroutines();
        HideAimLines();

        if (renderers != null)
            SetGlow(0f);
    }
}