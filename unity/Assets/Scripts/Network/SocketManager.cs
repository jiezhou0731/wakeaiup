using System;
using UnityEngine;
using WakeAIUp.Data;

namespace WakeAIUp.Network
{
    /// <summary>
    /// Manages Socket.IO connection to the game server.
    /// Dispatches strongly-typed C# events for each server message.
    ///
    /// Requires a Socket.IO Unity client library (e.g., SocketIOUnity or best-socket-io).
    /// This implementation provides the event interface; wire the actual socket
    /// library in ConnectToServer().
    /// </summary>
    public class SocketManager : MonoBehaviour
    {
        [Header("Connection")]
        public string serverUrl = "ws://localhost:3000";
        public bool autoConnect = true;
        public float reconnectDelay = 3f;

        // Events dispatched to game systems
        public event Action<GameStartMessage> OnGameStart;
        public event Action<RoundStartMessage> OnRoundStart;
        public event Action<PlayerSpeakMessage> OnPlayerSpeak;
        public event Action<int> OnVotePhaseStart;
        public event Action<PlayerVoteMessage> OnPlayerVote;
        public event Action<PlayerEliminatedMessage> OnPlayerEliminated;
        public event Action<GameEndMessage> OnGameEnd;
        public event Action<GameErrorMessage> OnGameError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private bool isConnected = false;

        private void Start()
        {
            if (autoConnect)
                ConnectToServer();
        }

        public void ConnectToServer()
        {
            // TODO: Initialize Socket.IO client library here
            // Example with SocketIOUnity:
            //
            // var uri = new Uri(serverUrl);
            // socket = new SocketIOUnity(uri, new SocketIOOptions {
            //     Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            // });
            //
            // socket.OnConnected += (sender, e) => {
            //     UnityMainThread.Enqueue(() => {
            //         isConnected = true;
            //         OnConnected?.Invoke();
            //     });
            // };
            //
            // RegisterListeners();
            // socket.Connect();

            Debug.Log($"[SocketManager] Connecting to {serverUrl}...");
        }

        private void RegisterListeners()
        {
            // TODO: Register socket event listeners when socket library is integrated
            // Example:
            //
            // socket.On("game_start", (response) => {
            //     var msg = JsonUtility.FromJson<GameStartMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnGameStart?.Invoke(msg));
            // });
            //
            // socket.On("round_start", (response) => {
            //     var msg = JsonUtility.FromJson<RoundStartMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnRoundStart?.Invoke(msg));
            // });
            //
            // socket.On("player_speak", (response) => {
            //     var msg = JsonUtility.FromJson<PlayerSpeakMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnPlayerSpeak?.Invoke(msg));
            // });
            //
            // socket.On("vote_phase_start", (response) => {
            //     int round = response.GetValue<int>();
            //     UnityMainThread.Enqueue(() => OnVotePhaseStart?.Invoke(round));
            // });
            //
            // socket.On("player_vote", (response) => {
            //     var msg = JsonUtility.FromJson<PlayerVoteMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnPlayerVote?.Invoke(msg));
            // });
            //
            // socket.On("player_eliminated", (response) => {
            //     var msg = JsonUtility.FromJson<PlayerEliminatedMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnPlayerEliminated?.Invoke(msg));
            // });
            //
            // socket.On("game_end", (response) => {
            //     var msg = JsonUtility.FromJson<GameEndMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnGameEnd?.Invoke(msg));
            // });
            //
            // socket.On("game_error", (response) => {
            //     var msg = JsonUtility.FromJson<GameErrorMessage>(response.GetValue<string>());
            //     UnityMainThread.Enqueue(() => OnGameError?.Invoke(msg));
            // });
        }

        public void RequestStart()
        {
            // socket?.Emit("request_start", "{}");
            Debug.Log("[SocketManager] Requesting game start...");
        }

        public void RequestRestart()
        {
            // socket?.Emit("request_restart", "{}");
            Debug.Log("[SocketManager] Requesting game restart...");
        }

        public void Disconnect()
        {
            // socket?.Disconnect();
            isConnected = false;
            OnDisconnected?.Invoke();
        }

        public bool IsConnected => isConnected;

        private void OnDestroy()
        {
            Disconnect();
        }

        // ──────────────────────────────────────────────
        //  DEBUG / TESTING - Simulate server events
        // ──────────────────────────────────────────────

        /// <summary>
        /// Call from editor or debug UI to simulate a full game sequence
        /// without a server connection.
        /// </summary>
        public void SimulateGameStart()
        {
            var msg = new GameStartMessage
            {
                round = 1,
                players = new PlayerInfo[]
                {
                    new PlayerInfo { id = 0, name = "Cypher" },
                    new PlayerInfo { id = 1, name = "Nova" },
                    new PlayerInfo { id = 2, name = "Echo" },
                    new PlayerInfo { id = 3, name = "Pulse" },
                    new PlayerInfo { id = 4, name = "Helix" },
                    new PlayerInfo { id = 5, name = "Drift" }
                }
            };
            OnGameStart?.Invoke(msg);
        }

        public void SimulatePlayerSpeak(int playerId, string playerName, string text)
        {
            var msg = new PlayerSpeakMessage
            {
                player_id = playerId,
                player_name = playerName,
                text = text,
                round = 1
            };
            OnPlayerSpeak?.Invoke(msg);
        }

        public void SimulatePlayerVote(int voterId, string voterName, int targetId, string targetName)
        {
            var msg = new PlayerVoteMessage
            {
                voter_id = voterId,
                voter_name = voterName,
                target_id = targetId,
                target_name = targetName,
                round = 1
            };
            OnPlayerVote?.Invoke(msg);
        }

        public void SimulateElimination(int playerId, string playerName, string role, string word)
        {
            var msg = new PlayerEliminatedMessage
            {
                player_id = playerId,
                player_name = playerName,
                role = role,
                word = word,
                votes_received = 3
            };
            OnPlayerEliminated?.Invoke(msg);
        }

        public void SimulateGameEnd(string winner)
        {
            var msg = new GameEndMessage
            {
                winner = winner,
                rounds_played = 3,
                spy_id = 2,
                spy_name = "Echo",
                civilian_word = "apple",
                spy_word = "pear",
                summary = "The spy was caught after 3 rounds of deduction."
            };
            OnGameEnd?.Invoke(msg);
        }
    }
}
