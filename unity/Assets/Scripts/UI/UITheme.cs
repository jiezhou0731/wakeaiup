using UnityEngine;

namespace WakeAIUp.UI
{
    /// <summary>
    /// Centralized light-theme color palette and styling constants.
    /// Sleek, high-tech aesthetic with cool blues, crisp whites, and luminous accents.
    /// </summary>
    public static class UITheme
    {
        // --- Core palette ---
        public static readonly Color Background = new Color(0.953f, 0.965f, 1f);         // #F3F7FF
        public static readonly Color Surface = Color.white;
        public static readonly Color SurfaceElevated = new Color(0.976f, 0.984f, 1f);    // #F9FBFF
        public static readonly Color Border = new Color(0.886f, 0.910f, 0.961f);         // #E2E8F5
        public static readonly Color BorderGlow = new Color(0.240f, 0.396f, 0.839f, 0.3f);

        // --- Text ---
        public static readonly Color TextPrimary = new Color(0.118f, 0.133f, 0.188f);    // #1E2230
        public static readonly Color TextSecondary = new Color(0.4f, 0.44f, 0.54f);
        public static readonly Color TextOnAccent = Color.white;

        // --- Accent colors ---
        public static readonly Color AccentBlue = new Color(0.240f, 0.396f, 0.839f);     // #3D65D6
        public static readonly Color AccentBlueLight = new Color(0.910f, 0.941f, 1f);    // #E8F0FF
        public static readonly Color AccentCyan = new Color(0.0f, 0.82f, 0.88f);         // #00D1E0
        public static readonly Color AccentPurple = new Color(0.478f, 0.318f, 0.898f);   // #7A51E5
        public static readonly Color AccentGreen = new Color(0.133f, 0.773f, 0.506f);    // #22C581
        public static readonly Color AccentRed = new Color(0.937f, 0.267f, 0.345f);      // #EF4458
        public static readonly Color AccentOrange = new Color(1f, 0.584f, 0.2f);         // #FF9533
        public static readonly Color AccentYellow = new Color(1f, 0.82f, 0.2f);

        // --- Player slot colors ---
        public static readonly Color[] PlayerColors = new Color[]
        {
            AccentBlue,
            AccentCyan,
            AccentPurple,
            AccentGreen,
            AccentOrange,
            new Color(0.85f, 0.25f, 0.60f) // Pink
        };

        // --- Glow / FX ---
        public static readonly Color GlowBlue = new Color(0.240f, 0.396f, 0.839f, 0.15f);
        public static readonly Color GlowCyan = new Color(0.0f, 0.82f, 0.88f, 0.12f);
        public static readonly Color EliminatedOverlay = new Color(0.85f, 0.86f, 0.89f, 0.7f);

        // --- Sizing ---
        public const float CardBorderRadius = 18f;
        public const float ButtonBorderRadius = 12f;
        public const float SmallBorderRadius = 8f;
        public const float CardBorderWidth = 2f;
        public const float SpeechBubblePadding = 16f;

        // --- Animation durations (seconds) ---
        public const float FadeInDuration = 0.4f;
        public const float SlideInDuration = 0.5f;
        public const float PulseDuration = 1.2f;
        public const float BounceInDuration = 0.6f;
        public const float TypewriterSpeed = 0.03f;
        public const float VoteLineDuration = 0.8f;
        public const float EliminateDuration = 0.7f;
        public const float PhaseTransitionDuration = 0.6f;

        // --- Animation curves ---
        public static AnimationCurve EaseOutBack
        {
            get
            {
                var curve = new AnimationCurve();
                curve.AddKey(new Keyframe(0f, 0f, 0f, 0f));
                curve.AddKey(new Keyframe(0.6f, 1.05f, 2f, 2f));
                curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
                return curve;
            }
        }

        public static AnimationCurve EaseOutElastic
        {
            get
            {
                var curve = new AnimationCurve();
                curve.AddKey(new Keyframe(0f, 0f));
                curve.AddKey(new Keyframe(0.3f, 1.1f));
                curve.AddKey(new Keyframe(0.5f, 0.95f));
                curve.AddKey(new Keyframe(0.7f, 1.02f));
                curve.AddKey(new Keyframe(1f, 1f));
                return curve;
            }
        }

        public static AnimationCurve SmoothStep
        {
            get
            {
                var curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                return curve;
            }
        }
    }
}
