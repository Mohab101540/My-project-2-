using TMPro;
using UnityEngine;

// On the ScorePopup prefab: the "+25" that floats up from a collected orb and fades out.
public class ScorePopup : MonoBehaviour
{
    const float Duration = 0.8f;
    const float Rise = 120f;

    public TextMeshProUGUI label;

    private Vector2 start;
    private float age;

    public static void Show(ScorePopup prefab, Canvas canvas, Vector3 worldPosition, string text)
    {
        if (prefab == null || canvas == null || Camera.main == null)
            return;

        ScorePopup popup = Instantiate(prefab, canvas.transform);
        popup.label.text = text;

        // Anchored bottom-left, so canvas units are screen pixels divided by the canvas scale.
        Vector2 screen = Camera.main.WorldToScreenPoint(worldPosition);
        popup.start = screen / canvas.scaleFactor;
        popup.label.rectTransform.anchoredPosition = popup.start;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = age / Duration;

        label.rectTransform.anchoredPosition = start + Vector2.up * Rise * t;
        label.alpha = 1f - t;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
