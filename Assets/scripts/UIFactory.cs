using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Small helpers for building menu UI from code.
// Used by Tools > Dodge Game > Set Up Menus and by the shop cards at runtime.
public static class UIFactory
{
    const int UILayer = 5;

    public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = UILayer;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    // Fills the whole parent.
    public static RectTransform Stretch(Transform parent, string name)
    {
        RectTransform rect = Rect(parent, name, Vector2.zero, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    // A full-screen panel; its Image also blocks taps from reaching the game UI behind it.
    public static GameObject Panel(Transform parent, string name, Color color)
    {
        RectTransform rect = Stretch(parent, name);
        rect.gameObject.AddComponent<Image>().color = color;
        return rect.gameObject;
    }

    public static Image Box(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        Image image = Rect(parent, name, anchor, position, size).gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    public static TextMeshProUGUI Text(Transform parent, string name, string text, float fontSize,
                                       Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        TextMeshProUGUI label = Rect(parent, name, anchor, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    public static Button Button(Transform parent, string name, string label, Vector2 anchor,
                                Vector2 position, Vector2 size, Color color, float fontSize = 52f)
    {
        Image image = Box(parent, name, anchor, position, size, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = Text(image.transform, "Label", label, fontSize, Vector2.zero, Vector2.zero, Vector2.zero, Color.white);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.fontStyle = FontStyles.Bold;

        return button;
    }
}
