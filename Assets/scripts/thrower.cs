using UnityEngine;

public class Thrower : MonoBehaviour
{
    public enum ThrowerType
    {
        Main,
        Red,
        Purple
    }

    [Header("Thrower Type")]
    public ThrowerType throwerType = ThrowerType.Main;

    [Header("Projectiles")]
    public GameObject normalProjectile;
    public GameObject purpleProjectile;
    public GameObject redProjectile;

    public Transform player;
    public Transform throwPoint;

    [Header("Throw Settings")]
    public float throwForce = 12f;
    public float purpleThrowForce = 28f;
    public float throwInterval = 2.5f;
    public float warningTime = 0.7f;

    [Header("Unlocked Projectiles")]
    public bool purpleUnlocked = false;
    public bool redUnlocked = false;

    private float throwTimer;
    private bool gameOver = false;
    private bool preparingThrow = false;
    private float preparationTimer;

    void Update()
    {
        if (gameOver)
            return;

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

        Debug.Log(gameObject.name + " is preparing to throw!");
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

        // MAIN THROWER
        else
        {
            int randomType = Random.Range(0, 100);

            if (redUnlocked && randomType < 20)
            {
                projectileToThrow = redProjectile;
                force = throwForce;
            }
            else if (purpleUnlocked && randomType < 50)
            {
                projectileToThrow = purpleProjectile;
                force = purpleThrowForce;
            }
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

    public void StopThrowing()
    {
        gameOver = true;
    }
}