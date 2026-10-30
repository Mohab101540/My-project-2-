using UnityEngine;

// Fits this RectTransform to the phone's safe area so notches and rounded corners never cover the HUD or FLIP.
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rect;
    private Rect applied;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        // Turning the phone the other way round moves the notch.
        if (Screen.safeArea != applied)
            Apply();
    }

    void Apply()
    {
        applied = Screen.safeArea;

        Vector2 min = applied.position;
        Vector2 max = applied.position + applied.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
