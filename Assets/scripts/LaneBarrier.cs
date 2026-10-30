using UnityEngine;

// A low block that slides along the player's lane. Solid to walk into but low enough to backflip over,
// and it turns around when it touches the player so it can never pin them against a wall.
[RequireComponent(typeof(Rigidbody))]
public class LaneBarrier : MonoBehaviour
{
    public float speed = 2.5f;
    public float limit = 7f; // GameManager sets this to the player's arenaLimit

    private Rigidbody rb;
    private float direction = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // Head towards the middle first.
        direction = rb.position.x > 0f ? -1f : 1f;
    }

    void FixedUpdate()
    {
        Vector3 next = rb.position + Vector3.right * direction * speed * Time.fixedDeltaTime;

        if (Mathf.Abs(next.x) > limit)
        {
            direction = -direction;
            next.x = Mathf.Clamp(next.x, -limit, limit);
        }

        rb.MovePosition(next);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionStay(collision);
    }

    // Stay as well as Enter: a player walking into it keeps one long contact,
    // and it must keep backing away for all of it, even after bouncing off a wall.
    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Back away from the player instead of pushing them.
        direction = collision.transform.position.x > rb.position.x ? -1f : 1f;
    }
}
