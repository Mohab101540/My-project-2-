using UnityEngine;

public class Collectible : MonoBehaviour
{
    public float value = 25f;
    public float lifetime = 6f;
    public float spinSpeed = 180f;

    private bool collected = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        // Guards against a second trigger from the same contact giving double score.
        collected = true;

        GameManager gameManager =
            FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.AddScore(value);
        }

        Destroy(gameObject);
    }
}
