using UnityEngine;

public class Thrower : MonoBehaviour
{
    public enum ThrowerType
    {
        Main,
        Red,
        Purple,
        Yellow,
        Blue,
        Bouncer
    }

    [Header("Thrower Type")]
    public ThrowerType throwerType = ThrowerType.Main;

    [Header("Projectiles")]
    public GameObject normalProjectile;
    public GameObject purpleProjectile;
    public GameObject redProjectile;
    public GameObject yellowProjectile;
    public GameObject blueProjectile;
    public GameObject bouncerProjectile;

    public Transform player;
    public Transform throwPoint;

    [Header("Throw Settings")]
    public float throwForce = 12f;
    public float purpleThrowForce = 28f;
    public float yellowThrowForce = 12f;
    public float blueThrowForce = 5f;
    public float bouncerThrowForce = 10f;

    public float throwInterval = 2.5f;
    public float warningTime = 0.7f;

    [Header("Unlocked Projectiles")]
    public bool purpleUnlocked = false;
    public bool redUnlocked = false;
    public bool yellowUnlocked = false;
    public bool blueUnlocked = false;
    public bool bouncerUnlocked = false;

    [Header("Explosive Lob")]
    public bool explosiveUnlocked = false;
    public float explosiveChance = 0.25f;
    public float explosiveFlightTime = 1.3f;

    [Header("Aiming")]
    [Tooltip("Turn smoothly to face the player (used by the black turrets).")]
    public bool turnToPlayer = false;
    public float turnSpeed = 180f;

    private float throwTimer;
    private bool gameOver = false;
    private bool preparingThrow = false;
    private float preparationTimer;

    void Update()
    {
        if (gameOver)
            return;

        if (turnToPlayer && player != null)
        {
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
        }

        if (!preparingThrow)
        {
            throwTimer += Time.deltaTime;

            if (throwTimer >= throwInterval)
            {
                StartThrow();
            }
        }
        else
        {
            preparationTimer += Time.deltaTime;

            if (preparationTimer >= warningTime)
            {
                ThrowProjectile();

                preparingThrow = false;
                preparationTimer = 0f;
                throwTimer = 0f;
            }
        }
    }

    void StartThrow()
    {
        preparingThrow = true;
        preparationTimer = 0f;
    }

    void ThrowProjectile()
    {
        GameObject projectileToThrow;
        float force;

        // RED THROWER
        if (throwerType == ThrowerType.Red)
        {
            projectileToThrow = redProjectile;
            force = throwForce;
        }

        // PURPLE THROWER
        else if (throwerType == ThrowerType.Purple)
        {
            projectileToThrow = purpleProjectile;
            force = purpleThrowForce;
        }

        // YELLOW THROWER
        else if (throwerType == ThrowerType.Yellow)
        {
            projectileToThrow = yellowProjectile;
            force = yellowThrowForce;
        }

        // BLUE THROWER
        else if (throwerType == ThrowerType.Blue)
        {
            projectileToThrow = blueProjectile;
            force = blueThrowForce;
        }

        // BOUNCER THROWER
        else if (throwerType == ThrowerType.Bouncer)
        {
            projectileToThrow = bouncerProjectile;
            force = bouncerThrowForce;
        }

        // MAIN THROWER
        else
        {
            if (explosiveUnlocked && Random.value < explosiveChance)
            {
                ThrowExplosive();
                return;
            }

            int randomType = Random.Range(0, 100);

            // RED = 20% chance
            if (redUnlocked && randomType < 20)
            {
                projectileToThrow = redProjectile;
                force = throwForce;
            }

            // PURPLE = 30% chance
            else if (purpleUnlocked && randomType < 50)
            {
                projectileToThrow = purpleProjectile;
                force = purpleThrowForce;
            }

            // YELLOW = 20% chance
            else if (yellowUnlocked && randomType < 70)
            {
                projectileToThrow = yellowProjectile;
                force = yellowThrowForce;
            }

            // BLUE = 15% chance
            else if (blueUnlocked && randomType < 85)
            {
                projectileToThrow = blueProjectile;
                force = blueThrowForce;
            }

            // BOUNCER = 10% chance
            else if (bouncerUnlocked && randomType < 95)
            {
                projectileToThrow = bouncerProjectile;
                force = bouncerThrowForce;
            }

            // NORMAL = remaining chance
            else
            {
                projectileToThrow = normalProjectile;
                force = throwForce;
            }
        }

        if (projectileToThrow == null)
        {
            Debug.LogError(gameObject.name + " has no projectile assigned!");
            return;
        }

        GameObject projectile = Instantiate(
            projectileToThrow,
            throwPoint.position,
            Quaternion.identity
        );

        Vector3 direction =
            (player.position - throwPoint.position).normalized;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * force;
        }
    }

    void ThrowExplosive()
    {
        if (normalProjectile == null)
        {
            Debug.LogError(gameObject.name + " has no normal projectile for the explosive lob!");
            return;
        }

        GameObject projectile = Instantiate(
            normalProjectile,
            throwPoint.position,
            Quaternion.identity
        );

        // Lands where the player stands now; the warning disc marks that spot.
        Vector3 target = new Vector3(player.position.x, 0f, player.position.z);

        Projectile explosive = projectile.GetComponent<Projectile>();

        if (explosive != null)
        {
            explosive.projectileType = Projectile.ProjectileType.Explosive;
            explosive.explosiveTarget = target;
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;

            float t = explosiveFlightTime;

            rb.linearVelocity =
                (target - throwPoint.position) / t -
                0.5f * Physics.gravity * t;
        }
    }

    public void UnlockExplosive()
    {
        explosiveUnlocked = true;
    }

    public void UnlockPurple()
    {
        purpleUnlocked = true;
        Debug.Log("PURPLE PROJECTILE UNLOCKED!");
    }

    public void UnlockRed()
    {
        redUnlocked = true;
        Debug.Log("RED PROJECTILE UNLOCKED!");
    }

    public void UnlockYellow()
    {
        yellowUnlocked = true;
        Debug.Log("YELLOW PROJECTILE UNLOCKED!");
    }

    public void UnlockBlue()
    {
        blueUnlocked = true;
        Debug.Log("BLUE PROJECTILE UNLOCKED!");
    }

    public void UnlockBouncer()
    {
        bouncerUnlocked = true;
        Debug.Log("BOUNCER PROJECTILE UNLOCKED!");
    }

    public void StopThrowing()
    {
        gameOver = true;
    }
}