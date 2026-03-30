using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeAIUp.Data;
using WakeAIUp.Network;
using WakeAIUp.UI;

namespace WakeAIUp.Game
{
    /// <summary>
    /// Singleton MonoBehaviour that orchestrates the game visualization.
    /// Manages 6 PlayerSlots in a circular layout, coordinates animations
    /// between game phases, and delegates UI updates to GameUIController.
    /// </summary>
    public class SpyGameManager : MonoBehaviour
    {
        public static SpyGameManager Instance { get; private set; }

        [Header("Components")]
        public GameUIController uiController;
        public SocketManager socketManager;

        [Header("Player Slots")]
        public PlayerSlot[] playerSlots = new PlayerSlot[6];
        public Dictionary<int, PlayerSlot> playerSlotMap = new Dictionary<int, PlayerSlot>();

        [Header("Layout")]
        public float circleRadius = 280f;
        public RectTransform playerContainer;

        private Canvas mainCanvas;
        private UIAnimator animator;
        private int currentRound = 0;
        private int currentSpeakerIndex = -1;
        private bool gameActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            animator = UIAnimator.Instance;
        }

        private void Start()
        {
            BuildScene();
            RegisterSocketEvents();
        }

        private void BuildScene()
        {
            // Create main canvas
            mainCanvas = UIFactory.CreateMainCanvas("GameCanvas");

            // Initialize UI controller
            var uiObj = new GameObject("GameUIController");
            uiObj.transform.SetParent(mainCanvas.transform, false);
            uiController = uiObj.AddComponent<GameUIController>();
            uiController.Initialize(mainCanvas);

            playerContainer = uiController.playerContainer;

            // Wire up start button
            uiController.startButton.onClick.AddListener(OnStartButtonClicked);

            // Start ambient scan line
            uiController.StartScanLineEffect();
        }

        private void RegisterSocketEvents()
        {
            if (socketManager == null)
            {
                var socketObj = new GameObject("SocketManager");
                socketObj.transform.SetParent(transform, false);
                socketManager = socketObj.AddComponent<SocketManager>();
            }

            socketManager.OnGameStart += HandleGameStart;
            socketManager.OnRoundStart += HandleRoundStart;
            socketManager.OnPlayerSpeak += HandlePlayerSpeak;
            socketManager.OnVotePhaseStart += HandleVotePhaseStart;
            socketManager.OnPlayerVote += HandlePlayerVote;
            socketManager.OnPlayerEliminated += HandlePlayerEliminated;
            socketManager.OnGameEnd += HandleGameEnd;
            socketManager.OnGameError += HandleGameError;
        }

        // ──────────────────────────────────────────────
        //  BUTTON HANDLERS
        // ──────────────────────────────────────────────

        private void OnStartButtonClicked()
        {
            socketManager.RequestStart();
            uiController.ShowStartButton(false);
            uiController.ShowAnnouncement("Connecting to server...", 2f);
        }

        // ──────────────────────────────────────────────
        //  SOCKET EVENT HANDLERS
        // ──────────────────────────────────────────────

        private void HandleGameStart(GameStartMessage msg)
        {
            gameActive = true;
            currentRound = msg.round;

            uiController.ShowStartButton(false);
            uiController.UpdateRound(currentRound);
            uiController.UpdatePhase("Starting");

            // Create player slots with staggered entrance
            StartCoroutine(CreatePlayerSlots(msg.players));
        }

        private IEnumerator CreatePlayerSlots(PlayerInfo[] players)
        {
            // Clear existing slots
            foreach (var slot in playerSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            playerSlotMap.Clear();

            int count = Mathf.Min(players.Length, 6);
            float containerWidth = playerContainer.rect.width;
            float containerHeight = playerContainer.rect.height;
            float radiusX = containerWidth * 0.38f;
            float radiusY = containerHeight * 0.35f;

            for (int i = 0; i < count; i++)
            {
                // Calculate circular position (start from top, go clockwise)
                float angle = (90f - i * (360f / count)) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(
                    Mathf.Cos(angle) * radiusX,
                    Mathf.Sin(angle) * radiusY
                );

                var slotObj = new GameObject($"Player_{players[i].name}");
                slotObj.transform.SetParent(playerContainer, false);

                var slotRect = slotObj.AddComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.anchoredPosition = pos;

                var slot = slotObj.AddComponent<PlayerSlot>();
                Color color = UITheme.PlayerColors[i % UITheme.PlayerColors.Length];
                slot.Initialize(players[i].id, players[i].name, color);

                playerSlots[i] = slot;
                playerSlotMap[players[i].id] = slot;

                // Stagger entrance
                yield return new WaitForSeconds(0.15f);
            }

            // Show initial announcement
            yield return new WaitForSeconds(0.3f);
            uiController.ShowAnnouncement("Game started! Players have received their secret words.", 3f);
        }

        private void HandleRoundStart(RoundStartMessage msg)
        {
            currentRound = msg.round;
            uiController.UpdateRound(currentRound);
            uiController.UpdatePhase(msg.phase);
            uiController.ClearVoteLines();

            // Hide previous speech bubbles
            foreach (var slot in playerSlots)
            {
                if (slot != null)
                    slot.HideSpeech();
            }

            string phaseDisplay = msg.phase == "describe" ? "Description" : msg.phase;
            uiController.ShowAnnouncement($"Round {msg.round} - {phaseDisplay} Phase", 2.5f);
        }

        private void HandlePlayerSpeak(PlayerSpeakMessage msg)
        {
            // Hide previous speaker's bubble
            if (currentSpeakerIndex >= 0 && currentSpeakerIndex < playerSlots.Length)
            {
                var prevSlot = playerSlots[currentSpeakerIndex];
                if (prevSlot != null)
                    prevSlot.HideSpeech();
            }

            if (playerSlotMap.TryGetValue(msg.player_id, out PlayerSlot slot))
            {
                slot.ShowSpeech(msg.text);

                // Find slot index for tracking
                for (int i = 0; i < playerSlots.Length; i++)
                {
                    if (playerSlots[i] == slot)
                    {
                        currentSpeakerIndex = i;
                        break;
                    }
                }
            }
        }

        private void HandleVotePhaseStart(int round)
        {
            uiController.UpdatePhase("Voting");
            uiController.ShowAnnouncement("Voting Phase - Who is the spy?", 2.5f);

            // Hide all speech bubbles with stagger
            StartCoroutine(HideAllSpeechBubbles());
        }

        private IEnumerator HideAllSpeechBubbles()
        {
            foreach (var slot in playerSlots)
            {
                if (slot != null && slot.isAlive)
                {
                    slot.HideSpeech();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private void HandlePlayerVote(PlayerVoteMessage msg)
        {
            if (playerSlotMap.TryGetValue(msg.voter_id, out PlayerSlot voterSlot) &&
                playerSlotMap.TryGetValue(msg.target_id, out PlayerSlot targetSlot))
            {
                // Animate vote indicator
                voterSlot.ShowVoteIndicator(targetSlot);

                // Draw vote line
                Vector3 from = voterSlot.rootRect.anchoredPosition;
                Vector3 to = targetSlot.rootRect.anchoredPosition;
                uiController.CreateVoteLine(from, to, voterSlot.playerColor);

                // Show announcement
                uiController.ShowAnnouncement($"{msg.voter_name} voted for {msg.target_name}", 2f);
            }
        }

        private void HandlePlayerEliminated(PlayerEliminatedMessage msg)
        {
            uiController.UpdatePhase("Elimination");

            if (playerSlotMap.TryGetValue(msg.player_id, out PlayerSlot slot))
            {
                slot.Eliminate();
            }

            uiController.ClearVoteLines();

            string roleDisplay = msg.role == "spy" ? "the Spy" : "a Civilian";
            string announcement = $"{msg.player_name} has been eliminated! They were {roleDisplay}.";
            if (!string.IsNullOrEmpty(msg.word))
                announcement += $" Their word was \"{msg.word}\".";

            uiController.ShowAnnouncement(announcement, 4f);
        }

        private void HandleGameEnd(GameEndMessage msg)
        {
            gameActive = false;
            currentSpeakerIndex = -1;

            // Short delay before showing result
            StartCoroutine(ShowGameEndSequence(msg));
        }

        private IEnumerator ShowGameEndSequence(GameEndMessage msg)
        {
            uiController.UpdatePhase("Game Over");
            yield return new WaitForSeconds(1.5f);

            uiController.ShowGameResult(msg);

            // Wire restart button
            uiController.restartButton.onClick.RemoveAllListeners();
            uiController.restartButton.onClick.AddListener(() =>
            {
                uiController.HideGameResult();
                ResetGame();
                socketManager.RequestRestart();
            });
        }

        private void HandleGameError(GameErrorMessage msg)
        {
            uiController.ShowAnnouncement($"Error: {msg.message}", 5f);
        }

        // ──────────────────────────────────────────────
        //  RESET
        // ──────────────────────────────────────────────

        private void ResetGame()
        {
            currentRound = 0;
            currentSpeakerIndex = -1;
            gameActive = false;

            foreach (var slot in playerSlots)
            {
                if (slot != null)
                    slot.Reset();
            }

            uiController.ResetUI();
            uiController.ShowStartButton(true);
        }

        private void OnDestroy()
        {
            if (socketManager != null)
            {
                socketManager.OnGameStart -= HandleGameStart;
                socketManager.OnRoundStart -= HandleRoundStart;
                socketManager.OnPlayerSpeak -= HandlePlayerSpeak;
                socketManager.OnVotePhaseStart -= HandleVotePhaseStart;
                socketManager.OnPlayerVote -= HandlePlayerVote;
                socketManager.OnPlayerEliminated -= HandlePlayerEliminated;
                socketManager.OnGameEnd -= HandleGameEnd;
                socketManager.OnGameError -= HandleGameError;
            }
        }
    }
}
