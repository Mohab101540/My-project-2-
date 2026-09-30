using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float arenaLimit = 7f;

    private bool touching = false;
    private Vector2 lastTouchPosition;

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
                Vector2 currentTouchPosition =
                    touch.position.ReadValue();

                if (!touching)
                {
                    touching = true;
                    lastTouchPosition = currentTouchPosition;
                }
                else
                {
                    float deltaX =
                        currentTouchPosition.x -
                        lastTouchPosition.x;

                    // Convert finger movement into player movement.
                    horizontal = Mathf.Clamp(
                        deltaX / 50f,
                        -1f,
                        1f
                    );

                    lastTouchPosition =
                        currentTouchPosition;
                }
            }
            else
            {
                touching = false;
            }
        }

        // =========================================
        // MOVE PLAYER
        // =========================================

        Vector3 movement =
            new Vector3(
                horizontal,
                0f,
                0f
            );

        transform.position +=
            movement *
            moveSpeed *
            Time.deltaTime;

        // =========================================
        // ARENA LIMIT
        // =========================================

        float clampedX =
            Mathf.Clamp(
                transform.position.x,
                -arenaLimit,
                arenaLimit
            );

        transform.position =
            new Vector3(
                clampedX,
                transform.position.y,
                transform.position.z
            );
    }
}