using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakeAIUp.UI
{
    /// <summary>
    /// Factory for creating themed UI elements at runtime.
    /// All elements follow the sleek, high-tech light theme.
    /// </summary>
    public static class UIFactory
    {
        public static Canvas CreateMainCanvas(string name = "MainCanvas")
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.color = bgColor;
            image.raycastTarget = false;

            return rect;
        }

        public static Image CreateRoundedPanel(Transform parent, string name, Color bgColor, float alpha = 1f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.color = new Color(bgColor.r, bgColor.g, bgColor.b, alpha);
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;

            return image;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            float fontSize = 18f, Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color ?? UITheme.TextPrimary;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            return tmp;
        }

        public static Button CreateButton(Transform parent, string name, string label,
            Color bgColor, Color textColor, float fontSize = 16f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.color = bgColor;
            image.type = Image.Type.Sliced;

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.88f, 0.95f);
            colors.fadeDuration = 0.15f;
            button.colors = colors;

            var textObj = CreateText(go.transform, "Label", label, fontSize, textColor);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 4);
            textRect.offsetMax = new Vector2(-16, -4);

            return button;
        }

        public static Image CreateGlowEffect(Transform parent, string name, Color glowColor, float size = 200f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);

            var image = go.AddComponent<Image>();
            image.color = glowColor;
            image.raycastTarget = false;

            return image;
        }

        public static Image CreateDivider(Transform parent, string name = "Divider", bool horizontal = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            if (horizontal)
                rect.sizeDelta = new Vector2(0, 1);
            else
                rect.sizeDelta = new Vector2(1, 0);

            var image = go.AddComponent<Image>();
            image.color = UITheme.Border;
            image.raycastTarget = false;

            return image;
        }

        public static RectTransform CreateCircularLayout(Transform parent, string name, int count, float radius)
        {
            var container = new GameObject(name);
            container.transform.SetParent(parent, false);
            var rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;

            for (int i = 0; i < count; i++)
            {
                float angle = (90f - i * (360f / count)) * Mathf.Deg2Rad;
                var slot = new GameObject($"Slot_{i}");
                slot.transform.SetParent(rect, false);
                var slotRect = slot.AddComponent<RectTransform>();
                slotRect.anchoredPosition = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
            }

            return rect;
        }
    }
}
