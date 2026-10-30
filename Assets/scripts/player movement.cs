using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Needed for UI button

// Rigidbody settings (gravity on, rotation + Z frozen, interpolate) live on the player in the scene.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float arenaLimit = 7f;
    public float jumpForce = 7f;
    // Renamed from jumpCooldown: the scene had kept an old 30 s value under that name.
    public float flipCooldown = 0.2f;
    public Button jumpButton; // The FLIP button under GameUI
    [Tooltip("How far (as a fraction of screen width) a finger must slide to start moving.")]
    public float swipeThreshold = 0.03f;

    private int moveTouchId = -1;
    private Vector2 touchStartPosition;
    private static readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private Rigidbody rb;
    private float lastFlipTime = -100f;
    private float horizontal = 0f;

    // Backflip: no invulnerability, the jump height is what dodges low projectiles.
    private bool flipping = false;
    private bool grounded = false;
    private float flipTimer = 0f;
    private float flipDuration = 1f;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        startRotation = transform.rotation;

        // A sleeping body sends no collision callbacks, so grounding would never be detected.
        rb.sleepThreshold = 0f;

        // Spin once over the whole time in the air.
        float takeoffSpeed = jumpForce / rb.mass;
        flipDuration = 2f * takeoffSpeed / Mathf.Abs(Physics.gravity.y);

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(TryJump);
        }
    }

    void Update()
    {
        horizontal = 0f;

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

            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                TryJump();
            }
        }

        // =========================================
        // TOUCH CONTROLS
        // =========================================
        // One finger steers. A finger that lands on a button (FLIP, pause) never steers,
        // so tapping FLIP doesn't move the player and a second finger can flip mid-swipe.
        if (Touchscreen.current != null)
        {
            bool steeringFingerDown = false;

            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed)
                    continue;

                int id = touch.touchId.ReadValue();
                Vector2 currentTouchPosition = touch.position.ReadValue();

                if (moveTouchId == -1 && touch.press.wasPressedThisFrame && !IsOverUI(currentTouchPosition))
                {
                    moveTouchId = id;
                    touchStartPosition = currentTouchPosition;
                }

                if (id != moveTouchId)
                    continue;

                steeringFingerDown = true;

                float threshold = Screen.width * swipeThreshold;
                float deltaX = currentTouchPosition.x - touchStartPosition.x;

                // The anchor trails the finger, so swiping back the other way reverses at once
                // and holding the finger still after a swipe keeps moving that way.
                if (deltaX >= threshold)
                {
                    horizontal = 1f;
                    touchStartPosition.x = currentTouchPosition.x - threshold;
                }
                else if (deltaX <= -threshold)
                {
                    horizontal = -1f;
                    touchStartPosition.x = currentTouchPosition.x + threshold;
                }
                else
                {
                    horizontal = 0f;
                }
            }

            if (!steeringFingerDown)
                moveTouchId = -1;
        }
    }

    bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
        EventSystem.current.RaycastAll(pointer, uiHits);
        return uiHits.Count > 0;
    }

    // =========================================
    // MOVE PLAYER (physics step)
    // =========================================
    void FixedUpdate()
    {
        if (rb == null)
            return;

        // Driving velocity keeps wall/projectile collisions in the physics engine
        // and leaves vertical velocity alone for the jump.
        rb.linearVelocity = new Vector3(
            horizontal * moveSpeed,
            rb.linearVelocity.y,
            0f
        );

        // =========================================
        // ARENA LIMIT
        // =========================================
        Vector3 position = rb.position;
        float clampedX = Mathf.Clamp(position.x, -arenaLimit, arenaLimit);

        if (clampedX != position.x)
        {
            rb.position = new Vector3(clampedX, position.y, position.z);
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // =========================================
        // BACKFLIP ROTATION
        // =========================================
        if (flipping)
        {
            flipTimer += Time.fixedDeltaTime;

            float progress = Mathf.Clamp01(flipTimer / flipDuration);

            // Negative X spins backwards for a player facing the camera.
            rb.MoveRotation(
                startRotation *
                Quaternion.Euler(-360f * progress, 0f, 0f)
            );

            // Landing ends the flip even if the spin isn't quite finished.
            bool landed = grounded && flipTimer > 0.2f;

            if (progress >= 1f || landed)
            {
                flipping = false;
                rb.MoveRotation(startRotation);
            }
        }

        // Collision callbacks after this physics step set it again while standing on something.
        grounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionStay(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                grounded = true;
                return;
            }
        }
    }

    bool IsGrounded()
    {
        return grounded && !flipping;
    }

    void TryJump()
    {
        bool cooledDown = Time.time - lastFlipTime >= flipCooldown;

        if (!cooledDown || rb == null || !IsGrounded())
            return;

        lastFlipTime = Time.time;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        flipping = true;
        flipTimer = 0f;
    }
}
