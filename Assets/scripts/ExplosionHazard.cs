using UnityEngine;

// Spawned by an explosive projectile: a short blast flash, then a fire patch that
// kills a grounded player for a few seconds. Jumping/backflipping over it is safe.
public class ExplosionHazard : MonoBehaviour
{
    public float blastRadius = 1.8f;
    public float flashTime = 0.25f;
    public float fireRadius = 1.1f;
    public float fireDuration = 2.5f;
    public float safeJumpHeight = 1.6f;

    private Transform flash;
    private Renderer fireRenderer;
    private Transform player;
    private GameManager gameManager;
    private float age = 0f;

    void Start()
    {
        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            player = playerMovement.transform;

        gameManager = FindFirstObjectByType<GameManager>();

        flash = MakeVisual(PrimitiveType.Sphere, new Color(1f, 0.85f, 0.3f)).transform;

        GameObject fire = MakeVisual(PrimitiveType.Cylinder, new Color(1f, 0.3f, 0f));
        fire.transform.localScale = new Vector3(fireRadius * 2f, 0.02f, fireRadius * 2f);
        fireRenderer = fire.GetComponent<Renderer>();

        Destroy(gameObject, fireDuration);
    }

    void Update()
    {
        age += Time.deltaTime;

        if (flash != null)
        {
            float t = age / flashTime;

            if (t >= 1f)
            {
                Destroy(flash.gameObject);
            }
            else
            {
                flash.localScale = Vector3.one * blastRadius * 2f * t;
            }
        }

        fireRenderer.material.color =
            Color.Lerp(
                new Color(1f, 0.3f, 0f),
                new Color(1f, 0.75f, 0f),
                Mathf.PingPong(age * 4f, 1f)
            );

        if (player == null || gameManager == null)
            return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;

        if (offset.magnitude < fireRadius &&
            player.position.y < safeJumpHeight)
        {
            gameManager.PlayerDied();
        }
    }

    GameObject MakeVisual(PrimitiveType shape, Color color)
    {
        GameObject visual = GameObject.CreatePrimitive(shape);

        // Visual only: damage is checked by distance, not physics.
        Destroy(visual.GetComponent<Collider>());

        visual.transform.SetParent(transform, false);
        visual.GetComponent<Renderer>().material.color = color;

        return visual;
    }
}
