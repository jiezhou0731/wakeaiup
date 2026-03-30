using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WakeAIUp.UI;

namespace WakeAIUp.Game
{
    /// <summary>
    /// Per-player visual representation with sleek light-theme styling.
    /// Includes avatar ring, name label, speech bubble, vote indicator,
    /// and elimination overlay — all with smooth animations.
    /// </summary>
    public class PlayerSlot : MonoBehaviour
    {
        [Header("References")]
        public RectTransform rootRect;
        public Image avatarRing;
        public Image avatarIcon;
        public TextMeshProUGUI nameLabel;
        public RectTransform speechBubbleRoot;
        public Image speechBubbleBg;
        public TextMeshProUGUI speechText;
        public CanvasGroup speechBubbleGroup;
        public Image eliminatedOverlay;
        public Image eliminatedX;
        public CanvasGroup slotCanvasGroup;
        public Image glowEffect;
        public Image statusIndicator;

        [Header("State")]
        public int playerId;
        public string playerName;
        public bool isAlive = true;
        public Color playerColor;

        private UIAnimator animator;
        private Coroutine activeHighlight;
        private Coroutine floatingCoroutine;
        private Coroutine glowCoroutine;
        private Vector2 originalPosition;

        public void Initialize(int id, string name, Color color)
        {
            playerId = id;
            playerName = name;
            playerColor = color;
            isAlive = true;
            animator = UIAnimator.Instance;

            BuildUI();
            originalPosition = rootRect.anchoredPosition;

            // Entrance animation
            rootRect.localScale = Vector3.zero;
            animator.BounceIn(rootRect, UITheme.BounceInDuration);

            // Subtle floating idle animation
            floatingCoroutine = animator.FloatingMotion(rootRect, 4f, 0.8f + Random.Range(0f, 0.4f));
        }

        private void BuildUI()
        {
            if (rootRect == null)
                rootRect = GetComponent<RectTransform>();
            if (rootRect == null)
                rootRect = gameObject.AddComponent<RectTransform>();

            rootRect.sizeDelta = new Vector2(160, 200);

            // Canvas group for fade effects
            if (slotCanvasGroup == null)
                slotCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Glow effect (behind avatar)
            var glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(transform, false);
            glowEffect = glowObj.AddComponent<Image>();
            glowEffect.color = new Color(playerColor.r, playerColor.g, playerColor.b, 0.08f);
            glowEffect.raycastTarget = false;
            var glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.sizeDelta = new Vector2(140, 140);
            glowRect.anchoredPosition = new Vector2(0, 20);

            // Avatar ring
            var ringObj = new GameObject("AvatarRing");
            ringObj.transform.SetParent(transform, false);
            avatarRing = ringObj.AddComponent<Image>();
            avatarRing.color = playerColor;
            avatarRing.raycastTarget = false;
            var ringRect = ringObj.GetComponent<RectTransform>();
            ringRect.sizeDelta = new Vector2(90, 90);
            ringRect.anchoredPosition = new Vector2(0, 30);

            // Avatar icon (inner circle)
            var iconObj = new GameObject("AvatarIcon");
            iconObj.transform.SetParent(ringObj.transform, false);
            avatarIcon = iconObj.AddComponent<Image>();
            avatarIcon.color = UITheme.Surface;
            avatarIcon.raycastTarget = false;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(78, 78);

            // Player initial text inside avatar
            var initialText = UIFactory.CreateText(iconObj.transform, "Initial",
                playerName.Length > 0 ? playerName[0].ToString().ToUpper() : "?",
                32f, playerColor, TextAlignmentOptions.Center);
            var initRect = initialText.GetComponent<RectTransform>();
            initRect.anchorMin = Vector2.zero;
            initRect.anchorMax = Vector2.one;
            initRect.offsetMin = Vector2.zero;
            initRect.offsetMax = Vector2.zero;
            initialText.fontStyle = FontStyles.Bold;

            // Name label
            nameLabel = UIFactory.CreateText(transform, "NameLabel", playerName,
                16f, UITheme.TextPrimary, TextAlignmentOptions.Center);
            nameLabel.fontStyle = FontStyles.Bold;
            var nameRect = nameLabel.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -25);
            nameRect.sizeDelta = new Vector2(150, 24);

            // Status indicator dot
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(ringObj.transform, false);
            statusIndicator = statusObj.AddComponent<Image>();
            statusIndicator.color = UITheme.AccentGreen;
            statusIndicator.raycastTarget = false;
            var statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.sizeDelta = new Vector2(14, 14);
            statusRect.anchoredPosition = new Vector2(32, -32);

            // Speech bubble (hidden by default)
            BuildSpeechBubble();

            // Eliminated overlay (hidden by default)
            BuildEliminatedOverlay();
        }

        private void BuildSpeechBubble()
        {
            var bubbleObj = new GameObject("SpeechBubble");
            bubbleObj.transform.SetParent(transform, false);
            speechBubbleRoot = bubbleObj.AddComponent<RectTransform>();
            speechBubbleRoot.anchoredPosition = new Vector2(0, 95);
            speechBubbleRoot.sizeDelta = new Vector2(220, 0);

            speechBubbleGroup = bubbleObj.AddComponent<CanvasGroup>();
            speechBubbleGroup.alpha = 0f;

            // Background
            speechBubbleBg = bubbleObj.AddComponent<Image>();
            speechBubbleBg.color = UITheme.Surface;
            speechBubbleBg.raycastTarget = false;

            // Border accent line at top
            var accentLine = new GameObject("AccentLine");
            accentLine.transform.SetParent(bubbleObj.transform, false);
            var lineImage = accentLine.AddComponent<Image>();
            lineImage.color = playerColor;
            lineImage.raycastTarget = false;
            var lineRect = accentLine.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 1);
            lineRect.anchorMax = new Vector2(1, 1);
            lineRect.pivot = new Vector2(0.5f, 1);
            lineRect.sizeDelta = new Vector2(0, 3);
            lineRect.offsetMin = new Vector2(0, lineRect.offsetMin.y);
            lineRect.offsetMax = new Vector2(0, lineRect.offsetMax.y);

            // Text
            speechText = UIFactory.CreateText(bubbleObj.transform, "SpeechText", "",
                14f, UITheme.TextPrimary, TextAlignmentOptions.TopLeft);
            var textRect = speechText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12, 8);
            textRect.offsetMax = new Vector2(-12, -12);
            speechText.enableWordWrapping = true;

            // Add layout for auto-sizing
            var fitter = bubbleObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            speechBubbleRoot.gameObject.SetActive(false);
        }

        private void BuildEliminatedOverlay()
        {
            var overlayObj = new GameObject("EliminatedOverlay");
            overlayObj.transform.SetParent(transform, false);
            eliminatedOverlay = overlayObj.AddComponent<Image>();
            eliminatedOverlay.color = UITheme.EliminatedOverlay;
            eliminatedOverlay.raycastTarget = false;
            var overlayRect = overlayObj.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // X mark
            var xObj = new GameObject("XMark");
            xObj.transform.SetParent(overlayObj.transform, false);
            eliminatedX = xObj.AddComponent<Image>();
            eliminatedX.color = UITheme.AccentRed;
            eliminatedX.raycastTarget = false;
            var xRect = xObj.GetComponent<RectTransform>();
            xRect.sizeDelta = new Vector2(50, 50);
            xRect.anchoredPosition = new Vector2(0, 30);

            // X text
            var xText = UIFactory.CreateText(xObj.transform, "X", "X",
                28f, UITheme.TextOnAccent, TextAlignmentOptions.Center);
            xText.fontStyle = FontStyles.Bold;
            var xTextRect = xText.GetComponent<RectTransform>();
            xTextRect.anchorMin = Vector2.zero;
            xTextRect.anchorMax = Vector2.one;
            xTextRect.offsetMin = Vector2.zero;
            xTextRect.offsetMax = Vector2.zero;

            overlayObj.SetActive(false);
        }

        // ──────────────────────────────────────────────
        //  PUBLIC API
        // ──────────────────────────────────────────────

        public void SetName(string name)
        {
            playerName = name;
            if (nameLabel != null) nameLabel.text = name;
        }

        public void ShowSpeech(string text)
        {
            if (!isAlive) return;

            speechBubbleRoot.gameObject.SetActive(true);
            animator.FadeIn(speechBubbleGroup, 0.3f);
            animator.PopIn(speechBubbleRoot, 0.35f);
            animator.TypewriterText(speechText, text, UITheme.TypewriterSpeed);

            // Highlight the player while speaking
            Highlight(true);
        }

        public void HideSpeech()
        {
            if (speechBubbleRoot == null) return;

            animator.FadeOut(speechBubbleGroup, 0.25f, () =>
            {
                speechBubbleRoot.gameObject.SetActive(false);
            });
            Highlight(false);
        }

        public void Highlight(bool active)
        {
            if (activeHighlight != null)
            {
                animator.StopCoroutine(activeHighlight);
                activeHighlight = null;
            }

            if (active)
            {
                // Pulsing glow
                glowCoroutine = animator.PulseGlow(glowEffect,
                    new Color(playerColor.r, playerColor.g, playerColor.b, 0.08f),
                    new Color(playerColor.r, playerColor.g, playerColor.b, 0.25f),
                    UITheme.PulseDuration, true);

                // Scale pulse
                activeHighlight = animator.PulseScale(rootRect, 1.0f, 1.08f, UITheme.PulseDuration, true);

                // Border glow on avatar ring
                animator.ColorTransition(avatarRing,
                    new Color(playerColor.r, playerColor.g, playerColor.b, 1f), 0.3f);
            }
            else
            {
                if (glowCoroutine != null)
                {
                    animator.StopCoroutine(glowCoroutine);
                    glowCoroutine = null;
                }

                rootRect.localScale = Vector3.one;
                glowEffect.color = new Color(playerColor.r, playerColor.g, playerColor.b, 0.08f);
            }
        }

        public void ShowVoteIndicator(PlayerSlot target)
        {
            if (!isAlive) return;
            animator.PunchScale(rootRect, 0.1f, 0.25f);
        }

        public void Eliminate()
        {
            isAlive = false;

            // Stop idle animations
            if (floatingCoroutine != null)
            {
                animator.StopCoroutine(floatingCoroutine);
                floatingCoroutine = null;
            }
            if (activeHighlight != null)
            {
                animator.StopCoroutine(activeHighlight);
                activeHighlight = null;
            }

            HideSpeech();

            // Elimination animation sequence
            animator.EliminateEffect(rootRect, slotCanvasGroup, UITheme.EliminateDuration, () =>
            {
                eliminatedOverlay.transform.parent.gameObject.SetActive(true);
                animator.FadeInGraphic(eliminatedOverlay, 0.3f);

                // Update status indicator
                statusIndicator.color = UITheme.AccentRed;
            });

            // Particle burst
            ParticleEffects.Instance.CreateEliminationBurst(
                transform.parent, rootRect.anchoredPosition, playerColor);
        }

        public void Reset()
        {
            isAlive = true;
            slotCanvasGroup.alpha = 1f;
            rootRect.localScale = Vector3.one;
            speechBubbleRoot.gameObject.SetActive(false);
            speechBubbleGroup.alpha = 0f;
            eliminatedOverlay.transform.parent.gameObject.SetActive(false);
            statusIndicator.color = UITheme.AccentGreen;
            glowEffect.color = new Color(playerColor.r, playerColor.g, playerColor.b, 0.08f);

            floatingCoroutine = animator.FloatingMotion(rootRect, 4f, 0.8f + Random.Range(0f, 0.4f));
        }
    }
}
