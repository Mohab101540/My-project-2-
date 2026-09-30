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

    private int attackPattern = 0;
    private bool attacking = false;
    private bool stopped = false;
    private float attackTimer = 0f;

    void Start()
    {
        attackTimer = attackInterval;

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

        switch (attackPattern)
        {
            case 0:
                SpreadAttack();
                break;

            case 1:
                yield return RapidAttack();
                break;

            case 2:
                SpreadAttack();

                yield return new WaitForSeconds(0.2f);

                SpreadAttack();
                break;

            case 3:
                BurstAttack();
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

    void SpreadAttack()
    {
        Debug.Log("BOSS ATTACK: SPREAD");

        Vector3 direction = GetPlayerDirection();

        float[] angles =
        {
            -50f,
            -25f,
            0f,
            25f,
            50f
        };

        foreach (float angle in angles)
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
        Debug.Log("BOSS ATTACK: RAPID FIRE");

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

    void BurstAttack()
    {
        Debug.Log("BOSS ATTACK: BURST");

        Vector3 playerDirection =
            GetPlayerDirection();

        FireProjectile(playerDirection);

        float[] angles =
        {
            -45f,
            45f,
            -90f,
            90f
        };

        foreach (float angle in angles)
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
        Debug.Log("BOSS ATTACK: DEATH BARRAGE");

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
    }
}