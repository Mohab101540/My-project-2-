using TMPro;
using UnityEngine;

// The "+25" that floats up from a collected orb and fades out.
public class ScorePopup : MonoBehaviour
{
    const float Duration = 0.8f;
    const float Rise = 120f;

    private TextMeshProUGUI label;
    private Vector2 start;
    private float age;

    public static void Show(Canvas canvas, Vector3 worldPosition, string text)
    {
        if (canvas == null || Camera.main == null)
            return;

        // Anchored bottom-left, so canvas units are screen pixels divided by the canvas scale.
        Vector2 screen = Camera.main.WorldToScreenPoint(worldPosition);
        TextMeshProUGUI label = UIFactory.Text(canvas.transform, "ScorePopup", text, 56f, Vector2.zero,
                                               screen / canvas.scaleFactor, new Vector2(240f, 70f),
                                               new Color(0.3f, 1f, 0.4f));
        label.fontStyle = FontStyles.Bold;

        ScorePopup popup = label.gameObject.AddComponent<ScorePopup>();
        popup.label = label;
        popup.start = label.rectTransform.anchoredPosition;
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
