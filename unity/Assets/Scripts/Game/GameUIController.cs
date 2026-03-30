using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WakeAIUp.UI;
using WakeAIUp.Data;

namespace WakeAIUp.Game
{
    /// <summary>
    /// Controls all HUD elements: round counter, phase indicator, announcements,
    /// start/restart button, game result panel, and vote lines.
    /// Built with a sleek light-theme, high-tech aesthetic and rich animations.
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("Top Bar")]
        public RectTransform topBar;
        public Image topBarBg;
        public TextMeshProUGUI roundLabel;
        public TextMeshProUGUI phaseLabel;
        public Image phaseIcon;
        public Image topBarShimmer;

        [Header("Center")]
        public RectTransform centerArea;
        public Image tableSurface;
        public RectTransform playerContainer;

        [Header("Announcement Banner")]
        public RectTransform announcementBanner;
        public Image announcementBg;
        public TextMeshProUGUI announcementText;
        public CanvasGroup announcementGroup;

        [Header("Start/Restart Button")]
        public Button startButton;
        public TextMeshProUGUI startButtonLabel;
        public Image startButtonBg;
        public Image startButtonGlow;

        [Header("Game Result Panel")]
        public RectTransform resultPanel;
        public Image resultPanelBg;
        public CanvasGroup resultPanelGroup;
        public TextMeshProUGUI resultTitle;
        public TextMeshProUGUI resultSubtitle;
        public TextMeshProUGUI resultDetails;
        public Button restartButton;

        [Header("Vote Lines")]
        public RectTransform voteLineContainer;
        public List<LineRenderer> activeVoteLines = new List<LineRenderer>();

        [Header("Scan Line Effect")]
        public RectTransform scanLine;
        public Image scanLineImage;

        private UIAnimator animator;
        private Canvas mainCanvas;
        private Coroutine scanLineCoroutine;
        private Coroutine buttonGlowCoroutine;

        public void Initialize(Canvas canvas)
        {
            mainCanvas = canvas;
            animator = UIAnimator.Instance;
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Background ──
            var bgPanel = UIFactory.CreatePanel(mainCanvas.transform, "Background", UITheme.Background);
            bgPanel.anchorMin = Vector2.zero;
            bgPanel.anchorMax = Vector2.one;
            bgPanel.offsetMin = Vector2.zero;
            bgPanel.offsetMax = Vector2.zero;

            // Add subtle grid pattern overlay
            var gridOverlay = UIFactory.CreateRoundedPanel(bgPanel, "GridOverlay",
                new Color(UITheme.AccentBlue.r, UITheme.AccentBlue.g, UITheme.AccentBlue.b, 0.02f));
            var gridRect = gridOverlay.GetComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            // ── Top Bar ──
            BuildTopBar();

            // ── Center Area with Table ──
            BuildCenterArea();

            // ── Announcement Banner ──
            BuildAnnouncementBanner();

            // ── Start Button ──
            BuildStartButton();

            // ── Result Panel ──
            BuildResultPanel();

            // ── Vote Line Container ──
            var voteObj = new GameObject("VoteLines");
            voteObj.transform.SetParent(mainCanvas.transform, false);
            voteLineContainer = voteObj.AddComponent<RectTransform>();
            voteLineContainer.anchorMin = Vector2.zero;
            voteLineContainer.anchorMax = Vector2.one;
            voteLineContainer.offsetMin = Vector2.zero;
            voteLineContainer.offsetMax = Vector2.zero;

            // ── Scan Line Effect ──
            BuildScanLine();

            // ── Ambient Particles ──
            ParticleEffects.Instance.CreateAmbientParticles(mainCanvas.transform);
        }

        private void BuildTopBar()
        {
            var barObj = new GameObject("TopBar");
            barObj.transform.SetParent(mainCanvas.transform, false);
            topBar = barObj.AddComponent<RectTransform>();
            topBar.anchorMin = new Vector2(0, 1);
            topBar.anchorMax = new Vector2(1, 1);
            topBar.pivot = new Vector2(0.5f, 1);
            topBar.sizeDelta = new Vector2(0, 72);
            topBar.offsetMin = new Vector2(0, topBar.offsetMin.y);
            topBar.offsetMax = new Vector2(0, 0);

            topBarBg = barObj.AddComponent<Image>();
            topBarBg.color = new Color(UITheme.Surface.r, UITheme.Surface.g, UITheme.Surface.b, 0.95f);
            topBarBg.raycastTarget = false;

            // Bottom border accent
            var borderLine = new GameObject("BottomBorder");
            borderLine.transform.SetParent(barObj.transform, false);
            var borderImg = borderLine.AddComponent<Image>();
            borderImg.color = UITheme.AccentBlue;
            borderImg.raycastTarget = false;
            var borderRect = borderLine.GetComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0, 0);
            borderRect.anchorMax = new Vector2(1, 0);
            borderRect.pivot = new Vector2(0.5f, 0);
            borderRect.sizeDelta = new Vector2(0, 2);

            // Round label
            roundLabel = UIFactory.CreateText(barObj.transform, "RoundLabel", "ROUND 1",
                14f, UITheme.TextSecondary, TextAlignmentOptions.MidlineLeft);
            roundLabel.fontStyle = FontStyles.Bold;
            roundLabel.characterSpacing = 4f;
            var roundRect = roundLabel.GetComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0, 0);
            roundRect.anchorMax = new Vector2(0, 1);
            roundRect.pivot = new Vector2(0, 0.5f);
            roundRect.offsetMin = new Vector2(32, 0);
            roundRect.offsetMax = new Vector2(200, 0);

            // Phase label (center)
            phaseLabel = UIFactory.CreateText(barObj.transform, "PhaseLabel", "WAITING",
                20f, UITheme.AccentBlue, TextAlignmentOptions.Center);
            phaseLabel.fontStyle = FontStyles.Bold;
            var phaseRect = phaseLabel.GetComponent<RectTransform>();
            phaseRect.anchorMin = new Vector2(0.3f, 0);
            phaseRect.anchorMax = new Vector2(0.7f, 1);
            phaseRect.offsetMin = Vector2.zero;
            phaseRect.offsetMax = Vector2.zero;

            // Title on right
            var titleLabel = UIFactory.CreateText(barObj.transform, "Title", "WHO IS THE SPY",
                12f, UITheme.TextSecondary, TextAlignmentOptions.MidlineRight);
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.characterSpacing = 6f;
            var titleRect = titleLabel.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(1, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(1, 0.5f);
            titleRect.offsetMin = new Vector2(-250, 0);
            titleRect.offsetMax = new Vector2(-32, 0);

            // Shimmer bar
            var shimmerObj = new GameObject("Shimmer");
            shimmerObj.transform.SetParent(barObj.transform, false);
            topBarShimmer = shimmerObj.AddComponent<Image>();
            topBarShimmer.color = new Color(1f, 1f, 1f, 0.06f);
            topBarShimmer.raycastTarget = false;
            var shimmerRect = shimmerObj.GetComponent<RectTransform>();
            shimmerRect.anchorMin = new Vector2(0, 0);
            shimmerRect.anchorMax = new Vector2(0, 1);
            shimmerRect.sizeDelta = new Vector2(60, 0);
            shimmerRect.offsetMin = new Vector2(shimmerRect.offsetMin.x, 0);
            shimmerRect.offsetMax = new Vector2(shimmerRect.offsetMax.x, 0);

            // Entrance animation
            animator.SlideFromTop(topBar, 72f, 0.5f);
        }

        private void BuildCenterArea()
        {
            var centerObj = new GameObject("CenterArea");
            centerObj.transform.SetParent(mainCanvas.transform, false);
            centerArea = centerObj.AddComponent<RectTransform>();
            centerArea.anchorMin = new Vector2(0.05f, 0.1f);
            centerArea.anchorMax = new Vector2(0.95f, 0.88f);
            centerArea.offsetMin = Vector2.zero;
            centerArea.offsetMax = Vector2.zero;

            // Central table surface
            var tableObj = new GameObject("Table");
            tableObj.transform.SetParent(centerObj.transform, false);
            tableSurface = tableObj.AddComponent<Image>();
            tableSurface.color = new Color(UITheme.SurfaceElevated.r, UITheme.SurfaceElevated.g,
                UITheme.SurfaceElevated.b, 0.6f);
            tableSurface.raycastTarget = false;
            var tableRect = tableObj.GetComponent<RectTransform>();
            tableRect.anchorMin = new Vector2(0.25f, 0.25f);
            tableRect.anchorMax = new Vector2(0.75f, 0.75f);
            tableRect.offsetMin = Vector2.zero;
            tableRect.offsetMax = Vector2.zero;

            // Table border ring
            var ringObj = new GameObject("TableRing");
            ringObj.transform.SetParent(tableObj.transform, false);
            var ringImage = ringObj.AddComponent<Image>();
            ringImage.color = UITheme.Border;
            ringImage.raycastTarget = false;
            var ringRect = ringObj.GetComponent<RectTransform>();
            ringRect.anchorMin = new Vector2(-0.05f, -0.05f);
            ringRect.anchorMax = new Vector2(1.05f, 1.05f);
            ringRect.offsetMin = Vector2.zero;
            ringRect.offsetMax = Vector2.zero;

            // Inner decorative ring
            var innerRingObj = new GameObject("InnerRing");
            innerRingObj.transform.SetParent(tableObj.transform, false);
            var innerRingImage = innerRingObj.AddComponent<Image>();
            innerRingImage.color = new Color(UITheme.AccentBlue.r, UITheme.AccentBlue.g, UITheme.AccentBlue.b, 0.08f);
            innerRingImage.raycastTarget = false;
            var innerRingRect = innerRingObj.GetComponent<RectTransform>();
            innerRingRect.anchorMin = new Vector2(0.1f, 0.1f);
            innerRingRect.anchorMax = new Vector2(0.9f, 0.9f);
            innerRingRect.offsetMin = Vector2.zero;
            innerRingRect.offsetMax = Vector2.zero;

            // Player container (overlay on center area for circular layout)
            var pcObj = new GameObject("PlayerContainer");
            pcObj.transform.SetParent(centerObj.transform, false);
            playerContainer = pcObj.AddComponent<RectTransform>();
            playerContainer.anchorMin = Vector2.zero;
            playerContainer.anchorMax = Vector2.one;
            playerContainer.offsetMin = Vector2.zero;
            playerContainer.offsetMax = Vector2.zero;
        }

        private void BuildAnnouncementBanner()
        {
            var bannerObj = new GameObject("AnnouncementBanner");
            bannerObj.transform.SetParent(mainCanvas.transform, false);
            announcementBanner = bannerObj.AddComponent<RectTransform>();
            announcementBanner.anchorMin = new Vector2(0.15f, 0.85f);
            announcementBanner.anchorMax = new Vector2(0.85f, 0.93f);
            announcementBanner.offsetMin = Vector2.zero;
            announcementBanner.offsetMax = Vector2.zero;

            announcementGroup = bannerObj.AddComponent<CanvasGroup>();
            announcementGroup.alpha = 0f;

            announcementBg = bannerObj.AddComponent<Image>();
            announcementBg.color = UITheme.Surface;
            announcementBg.raycastTarget = false;

            // Left accent bar
            var accentBar = new GameObject("AccentBar");
            accentBar.transform.SetParent(bannerObj.transform, false);
            var accentImg = accentBar.AddComponent<Image>();
            accentImg.color = UITheme.AccentBlue;
            accentImg.raycastTarget = false;
            var accentRect = accentBar.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0, 0);
            accentRect.anchorMax = new Vector2(0, 1);
            accentRect.pivot = new Vector2(0, 0.5f);
            accentRect.sizeDelta = new Vector2(4, 0);
            accentRect.offsetMin = new Vector2(0, 4);
            accentRect.offsetMax = new Vector2(4, -4);

            announcementText = UIFactory.CreateText(bannerObj.transform, "AnnouncementText", "",
                18f, UITheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
            var textRect = announcementText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 0);
            textRect.offsetMax = new Vector2(-20, 0);

            bannerObj.SetActive(false);
        }

        private void BuildStartButton()
        {
            var btnContainer = new GameObject("StartButtonContainer");
            btnContainer.transform.SetParent(mainCanvas.transform, false);
            var containerRect = btnContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.35f, 0.02f);
            containerRect.anchorMax = new Vector2(0.65f, 0.08f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Glow behind button
            var glowObj = new GameObject("ButtonGlow");
            glowObj.transform.SetParent(btnContainer.transform, false);
            startButtonGlow = glowObj.AddComponent<Image>();
            startButtonGlow.color = UITheme.GlowBlue;
            startButtonGlow.raycastTarget = false;
            var glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(-0.1f, -0.3f);
            glowRect.anchorMax = new Vector2(1.1f, 1.3f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;

            startButton = UIFactory.CreateButton(btnContainer.transform, "StartButton",
                "START GAME", UITheme.AccentBlue, UITheme.TextOnAccent, 16f);
            startButtonBg = startButton.GetComponent<Image>();
            startButtonLabel = startButton.GetComponentInChildren<TextMeshProUGUI>();
            startButtonLabel.fontStyle = FontStyles.Bold;
            startButtonLabel.characterSpacing = 4f;
            var btnRect = startButton.GetComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.one;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            // Pulsing glow animation
            buttonGlowCoroutine = animator.PulseGlow(startButtonGlow,
                UITheme.GlowBlue, new Color(UITheme.AccentBlue.r, UITheme.AccentBlue.g, UITheme.AccentBlue.b, 0.3f),
                2f, true);

            // Entrance animation
            animator.SlideFromBottom(containerRect, 50f, 0.6f);
        }

        private void BuildResultPanel()
        {
            var panelObj = new GameObject("ResultPanel");
            panelObj.transform.SetParent(mainCanvas.transform, false);
            resultPanel = panelObj.AddComponent<RectTransform>();
            resultPanel.anchorMin = new Vector2(0.2f, 0.15f);
            resultPanel.anchorMax = new Vector2(0.8f, 0.85f);
            resultPanel.offsetMin = Vector2.zero;
            resultPanel.offsetMax = Vector2.zero;

            resultPanelGroup = panelObj.AddComponent<CanvasGroup>();
            resultPanelGroup.alpha = 0f;

            // Semi-transparent backdrop
            var backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(mainCanvas.transform, false);
            backdrop.transform.SetSiblingIndex(panelObj.transform.GetSiblingIndex());
            var backdropImage = backdrop.AddComponent<Image>();
            backdropImage.color = new Color(0.95f, 0.96f, 1f, 0.85f);
            backdropImage.raycastTarget = true;
            var backdropRect = backdrop.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            backdrop.SetActive(false);

            resultPanelBg = panelObj.AddComponent<Image>();
            resultPanelBg.color = UITheme.Surface;
            resultPanelBg.raycastTarget = true;

            // Top accent gradient bar
            var topBar = new GameObject("TopAccent");
            topBar.transform.SetParent(panelObj.transform, false);
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = UITheme.AccentBlue;
            topBarImg.raycastTarget = false;
            var topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0, 1);
            topBarRect.anchorMax = new Vector2(1, 1);
            topBarRect.pivot = new Vector2(0.5f, 1);
            topBarRect.sizeDelta = new Vector2(0, 4);

            // Title
            resultTitle = UIFactory.CreateText(panelObj.transform, "ResultTitle", "GAME OVER",
                36f, UITheme.AccentBlue, TextAlignmentOptions.Center);
            resultTitle.fontStyle = FontStyles.Bold;
            var titleRect = resultTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.7f);
            titleRect.anchorMax = new Vector2(1, 0.9f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);

            // Subtitle
            resultSubtitle = UIFactory.CreateText(panelObj.transform, "ResultSubtitle", "",
                22f, UITheme.TextPrimary, TextAlignmentOptions.Center);
            var subRect = resultSubtitle.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0.55f);
            subRect.anchorMax = new Vector2(1, 0.7f);
            subRect.offsetMin = new Vector2(20, 0);
            subRect.offsetMax = new Vector2(-20, 0);

            // Details
            resultDetails = UIFactory.CreateText(panelObj.transform, "ResultDetails", "",
                16f, UITheme.TextSecondary, TextAlignmentOptions.Center);
            var detailRect = resultDetails.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0, 0.25f);
            detailRect.anchorMax = new Vector2(1, 0.55f);
            detailRect.offsetMin = new Vector2(30, 0);
            detailRect.offsetMax = new Vector2(-30, 0);

            // Restart button
            restartButton = UIFactory.CreateButton(panelObj.transform, "RestartButton",
                "PLAY AGAIN", UITheme.AccentBlue, UITheme.TextOnAccent, 16f);
            var restartLabel = restartButton.GetComponentInChildren<TextMeshProUGUI>();
            restartLabel.fontStyle = FontStyles.Bold;
            restartLabel.characterSpacing = 3f;
            var restartRect = restartButton.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.25f, 0.08f);
            restartRect.anchorMax = new Vector2(0.75f, 0.2f);
            restartRect.offsetMin = Vector2.zero;
            restartRect.offsetMax = Vector2.zero;

            panelObj.SetActive(false);
        }

        private void BuildScanLine()
        {
            var scanObj = new GameObject("ScanLine");
            scanObj.transform.SetParent(mainCanvas.transform, false);
            scanLine = scanObj.AddComponent<RectTransform>();
            scanLine.anchorMin = new Vector2(0, 0);
            scanLine.anchorMax = new Vector2(1, 0);
            scanLine.sizeDelta = new Vector2(0, 2);

            scanLineImage = scanObj.AddComponent<Image>();
            scanLineImage.color = new Color(UITheme.AccentCyan.r, UITheme.AccentCyan.g, UITheme.AccentCyan.b, 0.08f);
            scanLineImage.raycastTarget = false;
        }

        // ──────────────────────────────────────────────
        //  PUBLIC API
        // ──────────────────────────────────────────────

        public void UpdateRound(int round)
        {
            roundLabel.text = $"ROUND {round}";
            animator.PunchScale(roundLabel.GetComponent<RectTransform>(), 0.12f, 0.3f);
        }

        public void UpdatePhase(string phase)
        {
            string displayPhase = phase.ToUpper();
            phaseLabel.text = displayPhase;

            // Phase transition animation
            var phaseRect = phaseLabel.GetComponent<RectTransform>();
            animator.PopIn(phaseRect, UITheme.PhaseTransitionDuration);

            // Color-code phases
            switch (phase.ToLower())
            {
                case "describe":
                case "description":
                    phaseLabel.color = UITheme.AccentBlue;
                    break;
                case "vote":
                case "voting":
                    phaseLabel.color = UITheme.AccentPurple;
                    break;
                case "elimination":
                    phaseLabel.color = UITheme.AccentRed;
                    break;
                default:
                    phaseLabel.color = UITheme.AccentBlue;
                    break;
            }

            // Run shimmer across top bar
            if (topBarShimmer != null)
                animator.ShimmerEffect(topBarShimmer.GetComponent<RectTransform>(), topBar, 1.2f, false);
        }

        public void ShowAnnouncement(string text, float displayTime = 3.5f)
        {
            announcementBanner.gameObject.SetActive(true);
            announcementText.text = text;
            animator.ShowNotification(announcementBanner, displayTime, 40f, 0.4f);
        }

        public void ShowStartButton(bool show)
        {
            startButton.transform.parent.gameObject.SetActive(show);
            if (show)
            {
                animator.SlideFromBottom(startButton.transform.parent.GetComponent<RectTransform>(), 40f, 0.4f);
            }
        }

        public void ShowGameResult(GameEndMessage result)
        {
            resultPanel.gameObject.SetActive(true);
            resultPanelGroup.alpha = 0f;

            bool civilianWin = result.winner == "civilian";
            resultTitle.text = civilianWin ? "CIVILIANS WIN" : "SPY WINS";
            resultTitle.color = civilianWin ? UITheme.AccentGreen : UITheme.AccentRed;

            resultSubtitle.text = civilianWin
                ? $"The spy {result.spy_name} has been caught!"
                : $"{result.spy_name} fooled everyone!";

            resultDetails.text = $"Rounds Played: {result.rounds_played}\n" +
                $"Civilian Word: {result.civilian_word}\n" +
                $"Spy Word: {result.spy_word}";

            // Animated entrance
            animator.FadeIn(resultPanelGroup, 0.4f);
            animator.BounceIn(resultPanel, 0.6f);

            // Victory confetti for civilian win
            if (civilianWin)
                ParticleEffects.Instance.CreateVictoryConfetti(mainCanvas.transform);
        }

        public void HideGameResult()
        {
            animator.FadeOut(resultPanelGroup, 0.3f, () =>
            {
                resultPanel.gameObject.SetActive(false);
            });
        }

        public LineRenderer CreateVoteLine(Vector3 from, Vector3 to, Color color)
        {
            var lineObj = new GameObject("VoteLine");
            lineObj.transform.SetParent(voteLineContainer, false);

            var line = lineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.material = new Material(Shader.Find("UI/Default"));
            line.sortingOrder = 5;

            animator.AnimateVoteLine(line, from, to, color);
            activeVoteLines.Add(line);

            return line;
        }

        public void ClearVoteLines()
        {
            foreach (var line in activeVoteLines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            activeVoteLines.Clear();
        }

        public void StartScanLineEffect()
        {
            if (scanLineCoroutine != null)
                animator.StopCoroutine(scanLineCoroutine);

            scanLineCoroutine = animator.ShimmerEffect(scanLine,
                mainCanvas.GetComponent<RectTransform>(), 3f, true);
        }

        public void ResetUI()
        {
            ClearVoteLines();
            HideGameResult();
            announcementGroup.alpha = 0f;
            announcementBanner.gameObject.SetActive(false);
            roundLabel.text = "ROUND 1";
            phaseLabel.text = "WAITING";
            phaseLabel.color = UITheme.AccentBlue;
        }
    }
}
