using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Needed for UI button

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float arenaLimit = 7f;
    public float jumpForce = 7f;
    public Button jumpButton; // Assign in Inspector

    private bool touching = false;
    private Vector2 touchStartPosition;
    private Rigidbody rb;
    private bool canJump = true;
    private float jumpCooldown = 30f;
    private float jumpTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(TryJump);
        }
    }

    void Update()
    {
        float horizontal = 0f;

        // =========================================
        // KEYBOARD CONTROLS
        // =========================================
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal = -1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal = 1f;
            }
        }

        // =========================================
        // TOUCH CONTROLS
        // =========================================
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.isPressed)
            {
                Vector2 currentTouchPosition = touch.position.ReadValue();

                if (!touching)
                {
                    touching = true;
                    touchStartPosition = currentTouchPosition;
                }

                float deltaX = currentTouchPosition.x - touchStartPosition.x;

                if (deltaX > 20f) horizontal = 1f;
                else if (deltaX < -20f) horizontal = -1f;
                else horizontal = 0f;
            }
            else
            {
                touching = false;
            }
        }

        // =========================================
        // MOVE PLAYER
        // =========================================
        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        transform.position += movement * moveSpeed * Time.deltaTime;

        // =========================================
        // ARENA LIMIT
        // =========================================
        float clampedX = Mathf.Clamp(transform.position.x, -arenaLimit, arenaLimit);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // =========================================
        // JUMP COOLDOWN TIMER
        // =========================================
        if (!canJump)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= jumpCooldown)
            {
                canJump = true;
                jumpTimer = 0f;
            }
        }
    }

    void TryJump()
    {
        if (canJump && rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }
    }
}
