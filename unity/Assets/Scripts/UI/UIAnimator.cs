using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WakeAIUp.UI
{
    /// <summary>
    /// Provides sleek, high-tech UI animations: fades, slides, bounces,
    /// typewriter text, pulsing glows, ripple effects, and more.
    /// Attach to any GameObject to use as a coroutine runner.
    /// </summary>
    public class UIAnimator : MonoBehaviour
    {
        private static UIAnimator _instance;
        public static UIAnimator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("UIAnimator");
                    _instance = go.AddComponent<UIAnimator>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // ──────────────────────────────────────────────
        //  FADE
        // ──────────────────────────────────────────────

        public Coroutine FadeIn(CanvasGroup group, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.FadeInDuration;
            return StartCoroutine(FadeCanvasGroup(group, 0f, 1f, duration, onComplete));
        }

        public Coroutine FadeOut(CanvasGroup group, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.FadeInDuration;
            return StartCoroutine(FadeCanvasGroup(group, 1f, 0f, duration, onComplete));
        }

        public Coroutine FadeInGraphic(Graphic graphic, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.FadeInDuration;
            return StartCoroutine(FadeGraphicAlpha(graphic, 0f, 1f, duration, onComplete));
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration, Action onComplete)
        {
            float elapsed = 0f;
            group.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = UITheme.SmoothStep.Evaluate(Mathf.Clamp01(elapsed / duration));
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            group.alpha = to;
            onComplete?.Invoke();
        }

        private IEnumerator FadeGraphicAlpha(Graphic graphic, float from, float to, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Color c = graphic.color;
            c.a = from;
            graphic.color = c;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = UITheme.SmoothStep.Evaluate(Mathf.Clamp01(elapsed / duration));
                c.a = Mathf.Lerp(from, to, t);
                graphic.color = c;
                yield return null;
            }
            c.a = to;
            graphic.color = c;
            onComplete?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  SLIDE
        // ──────────────────────────────────────────────

        public Coroutine SlideIn(RectTransform rect, Vector2 fromOffset, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.SlideInDuration;
            return StartCoroutine(SlideAnimation(rect, fromOffset, Vector2.zero, duration, onComplete));
        }

        public Coroutine SlideFromBottom(RectTransform rect, float distance = 80f, float duration = -1f, Action onComplete = null)
        {
            return SlideIn(rect, new Vector2(0, -distance), duration, onComplete);
        }

        public Coroutine SlideFromLeft(RectTransform rect, float distance = 120f, float duration = -1f, Action onComplete = null)
        {
            return SlideIn(rect, new Vector2(-distance, 0), duration, onComplete);
        }

        public Coroutine SlideFromRight(RectTransform rect, float distance = 120f, float duration = -1f, Action onComplete = null)
        {
            return SlideIn(rect, new Vector2(distance, 0), duration, onComplete);
        }

        public Coroutine SlideFromTop(RectTransform rect, float distance = 80f, float duration = -1f, Action onComplete = null)
        {
            return SlideIn(rect, new Vector2(0, distance), duration, onComplete);
        }

        private IEnumerator SlideAnimation(RectTransform rect, Vector2 fromOffset, Vector2 toOffset, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition + fromOffset;
            Vector2 endPos = rect.anchoredPosition + toOffset;
            var curve = UITheme.EaseOutBack;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
                yield return null;
            }
            rect.anchoredPosition = endPos;
            onComplete?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  SCALE / BOUNCE
        // ──────────────────────────────────────────────

        public Coroutine BounceIn(RectTransform rect, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.BounceInDuration;
            return StartCoroutine(ScaleAnimation(rect, Vector3.zero, Vector3.one, duration, UITheme.EaseOutElastic, onComplete));
        }

        public Coroutine PopIn(RectTransform rect, float duration = 0.3f, Action onComplete = null)
        {
            return StartCoroutine(ScaleAnimation(rect, Vector3.one * 0.8f, Vector3.one, duration, UITheme.EaseOutBack, onComplete));
        }

        public Coroutine ShrinkOut(RectTransform rect, float duration = 0.3f, Action onComplete = null)
        {
            return StartCoroutine(ScaleAnimation(rect, Vector3.one, Vector3.zero, duration, UITheme.SmoothStep, onComplete));
        }

        public Coroutine PunchScale(RectTransform rect, float intensity = 0.15f, float duration = 0.3f)
        {
            return StartCoroutine(PunchScaleRoutine(rect, intensity, duration));
        }

        private IEnumerator ScaleAnimation(RectTransform rect, Vector3 from, Vector3 to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;
            rect.localScale = from;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                rect.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }
            rect.localScale = to;
            onComplete?.Invoke();
        }

        private IEnumerator PunchScaleRoutine(RectTransform rect, float intensity, float duration)
        {
            float elapsed = 0f;
            Vector3 original = rect.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float wave = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * intensity;
                rect.localScale = original + Vector3.one * wave;
                yield return null;
            }
            rect.localScale = original;
        }

        // ──────────────────────────────────────────────
        //  TYPEWRITER TEXT
        // ──────────────────────────────────────────────

        public Coroutine TypewriterText(TextMeshProUGUI textComponent, string fullText,
            float charDelay = -1f, Action onComplete = null)
        {
            if (charDelay < 0) charDelay = UITheme.TypewriterSpeed;
            return StartCoroutine(TypewriterRoutine(textComponent, fullText, charDelay, onComplete));
        }

        private IEnumerator TypewriterRoutine(TextMeshProUGUI textComponent, string fullText, float charDelay, Action onComplete)
        {
            textComponent.text = "";
            textComponent.maxVisibleCharacters = 0;
            textComponent.text = fullText;

            for (int i = 0; i <= fullText.Length; i++)
            {
                textComponent.maxVisibleCharacters = i;
                yield return new WaitForSeconds(charDelay);
            }
            onComplete?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  PULSE / GLOW
        // ──────────────────────────────────────────────

        public Coroutine PulseGlow(Image image, Color baseColor, Color glowColor, float duration = -1f, bool loop = true)
        {
            if (duration < 0) duration = UITheme.PulseDuration;
            return StartCoroutine(PulseGlowRoutine(image, baseColor, glowColor, duration, loop));
        }

        public Coroutine PulseScale(RectTransform rect, float minScale = 0.95f, float maxScale = 1.05f,
            float duration = -1f, bool loop = true)
        {
            if (duration < 0) duration = UITheme.PulseDuration;
            return StartCoroutine(PulseScaleRoutine(rect, minScale, maxScale, duration, loop));
        }

        private IEnumerator PulseGlowRoutine(Image image, Color baseColor, Color glowColor, float duration, bool loop)
        {
            do
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = (Mathf.Sin(elapsed / duration * Mathf.PI * 2f - Mathf.PI / 2f) + 1f) / 2f;
                    image.color = Color.Lerp(baseColor, glowColor, t);
                    yield return null;
                }
            } while (loop);
        }

        private IEnumerator PulseScaleRoutine(RectTransform rect, float minScale, float maxScale, float duration, bool loop)
        {
            do
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = (Mathf.Sin(elapsed / duration * Mathf.PI * 2f - Mathf.PI / 2f) + 1f) / 2f;
                    float scale = Mathf.Lerp(minScale, maxScale, t);
                    rect.localScale = Vector3.one * scale;
                    yield return null;
                }
            } while (loop);
        }

        // ──────────────────────────────────────────────
        //  VOTE LINE ANIMATION
        // ──────────────────────────────────────────────

        public Coroutine AnimateVoteLine(LineRenderer line, Vector3 start, Vector3 end,
            Color color, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.VoteLineDuration;
            return StartCoroutine(VoteLineRoutine(line, start, end, color, duration, onComplete));
        }

        private IEnumerator VoteLineRoutine(LineRenderer line, Vector3 start, Vector3 end, Color color, float duration, Action onComplete)
        {
            line.positionCount = 2;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.4f);
            line.startWidth = 3f;
            line.endWidth = 1.5f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = UITheme.SmoothStep.Evaluate(Mathf.Clamp01(elapsed / duration));
                line.SetPosition(0, start);
                line.SetPosition(1, Vector3.Lerp(start, end, t));
                yield return null;
            }
            line.SetPosition(1, end);
            onComplete?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  ELIMINATION EFFECT
        // ──────────────────────────────────────────────

        public Coroutine EliminateEffect(RectTransform rect, CanvasGroup group, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = UITheme.EliminateDuration;
            return StartCoroutine(EliminateRoutine(rect, group, duration, onComplete));
        }

        private IEnumerator EliminateRoutine(RectTransform rect, CanvasGroup group, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector3 originalScale = rect.localScale;
            float shakeIntensity = 8f;

            // Phase 1: Shake (first 40%)
            float shakeEnd = duration * 0.4f;
            while (elapsed < shakeEnd)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / shakeEnd;
                float shake = Mathf.Sin(t * Mathf.PI * 12f) * shakeIntensity * (1f - t);
                rect.anchoredPosition += new Vector2(shake, 0);
                yield return null;
                rect.anchoredPosition -= new Vector2(shake, 0);
            }

            // Phase 2: Fade + shrink (remaining 60%)
            float fadeStart = elapsed;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01((elapsed - fadeStart) / (duration - fadeStart));
                group.alpha = Mathf.Lerp(1f, 0.4f, t);
                rect.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, t);
                yield return null;
            }
            onComplete?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  SHIMMER / SCANNING LINE
        // ──────────────────────────────────────────────

        public Coroutine ShimmerEffect(RectTransform shimmerBar, RectTransform container,
            float duration = 1.5f, bool loop = true)
        {
            return StartCoroutine(ShimmerRoutine(shimmerBar, container, duration, loop));
        }

        private IEnumerator ShimmerRoutine(RectTransform shimmerBar, RectTransform container, float duration, bool loop)
        {
            float width = container.rect.width;
            do
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float x = Mathf.Lerp(-width * 0.5f - 60f, width * 0.5f + 60f, t);
                    shimmerBar.anchoredPosition = new Vector2(x, 0);
                    yield return null;
                }
            } while (loop);
        }

        // ──────────────────────────────────────────────
        //  COLOR TRANSITION
        // ──────────────────────────────────────────────

        public Coroutine ColorTransition(Graphic graphic, Color toColor, float duration = 0.3f, Action onComplete = null)
        {
            return StartCoroutine(ColorTransitionRoutine(graphic, toColor, duration, onComplete));
        }

        private IEnumerator ColorTransitionRoutine(Graphic graphic, Color toColor, float duration, Action onComplete)
        {
            Color fromColor = graphic.color;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = UITheme.SmoothStep.Evaluate(Mathf.Clamp01(elapsed / duration));
                graphic.color = Color.Lerp(fromColor, toColor, t);
                yield return null;
            }
            graphic.color = toColor;
            onComplete?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  FLOATING / IDLE MOTION
        // ──────────────────────────────────────────────

        public Coroutine FloatingMotion(RectTransform rect, float amplitude = 6f, float speed = 1f)
        {
            return StartCoroutine(FloatingRoutine(rect, amplitude, speed));
        }

        private IEnumerator FloatingRoutine(RectTransform rect, float amplitude, float speed)
        {
            Vector2 origin = rect.anchoredPosition;
            float offset = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            while (true)
            {
                float y = Mathf.Sin(Time.time * speed + offset) * amplitude;
                rect.anchoredPosition = origin + new Vector2(0, y);
                yield return null;
            }
        }

        // ──────────────────────────────────────────────
        //  STAGGERED ENTRANCE
        // ──────────────────────────────────────────────

        public Coroutine StaggeredEntrance(RectTransform[] items, float staggerDelay = 0.1f,
            float itemDuration = 0.4f)
        {
            return StartCoroutine(StaggeredEntranceRoutine(items, staggerDelay, itemDuration));
        }

        private IEnumerator StaggeredEntranceRoutine(RectTransform[] items, float staggerDelay, float itemDuration)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                item.localScale = Vector3.zero;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                BounceIn(items[i], itemDuration);
                yield return new WaitForSeconds(staggerDelay);
            }
        }

        // ──────────────────────────────────────────────
        //  NOTIFICATION BANNER
        // ──────────────────────────────────────────────

        public Coroutine ShowNotification(RectTransform banner, float displayTime = 3f,
            float slideDistance = 60f, float slideDuration = 0.4f)
        {
            return StartCoroutine(NotificationRoutine(banner, displayTime, slideDistance, slideDuration));
        }

        private IEnumerator NotificationRoutine(RectTransform banner, float displayTime, float slideDistance, float slideDuration)
        {
            var group = banner.GetComponent<CanvasGroup>();
            if (group == null) group = banner.gameObject.AddComponent<CanvasGroup>();

            group.alpha = 0f;
            banner.anchoredPosition += new Vector2(0, slideDistance);

            // Slide in
            float elapsed = 0f;
            Vector2 startPos = banner.anchoredPosition;
            Vector2 endPos = startPos - new Vector2(0, slideDistance);
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = UITheme.EaseOutBack.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
                banner.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
                group.alpha = Mathf.Clamp01(elapsed / (slideDuration * 0.5f));
                yield return null;
            }
            banner.anchoredPosition = endPos;
            group.alpha = 1f;

            yield return new WaitForSeconds(displayTime);

            // Slide out
            elapsed = 0f;
            startPos = banner.anchoredPosition;
            endPos = startPos + new Vector2(0, slideDistance);
            while (elapsed < slideDuration * 0.7f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / (slideDuration * 0.7f));
                banner.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                group.alpha = 1f - t;
                yield return null;
            }
            group.alpha = 0f;
        }
    }
}
