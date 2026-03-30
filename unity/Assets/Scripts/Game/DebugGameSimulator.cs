using System.Collections;
using UnityEngine;
using WakeAIUp.Network;

namespace WakeAIUp.Game
{
    /// <summary>
    /// Debug tool to simulate a full game sequence for testing UI animations
    /// without a running server. Attach to any GameObject in the scene
    /// and press Space to start the simulation.
    /// </summary>
    public class DebugGameSimulator : MonoBehaviour
    {
        private SocketManager socket;
        private bool isRunning = false;

        private readonly string[] playerNames = { "Cypher", "Nova", "Echo", "Pulse", "Helix", "Drift" };

        private readonly string[] descriptions = {
            "It's round and you can hold it in one hand. People enjoy it fresh.",
            "You can find it growing in an orchard. It comes in many varieties.",
            "It's commonly associated with autumn. Very popular as a snack.",
            "It has a thin skin and is quite juicy when ripe.",
            "Many cultures have stories and symbols about this item.",
            "It makes a satisfying crunch when you bite into it."
        };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isRunning)
            {
                isRunning = true;
                StartCoroutine(RunSimulation());
            }
        }

        private IEnumerator RunSimulation()
        {
            yield return new WaitForSeconds(0.5f);

            socket = SpyGameManager.Instance?.socketManager;
            if (socket == null)
            {
                Debug.LogError("[DebugSimulator] No SocketManager found.");
                isRunning = false;
                yield break;
            }

            // Game start
            Debug.Log("[DebugSimulator] Starting simulated game...");
            socket.SimulateGameStart();
            yield return new WaitForSeconds(2.5f);

            // Round 1 - Description phase
            for (int i = 0; i < 6; i++)
            {
                socket.SimulatePlayerSpeak(i, playerNames[i], descriptions[i]);
                yield return new WaitForSeconds(3f);
            }

            yield return new WaitForSeconds(1f);

            // Round 1 - Voting phase
            socket.SimulatePlayerVote(0, "Cypher", 2, "Echo");
            yield return new WaitForSeconds(1.5f);
            socket.SimulatePlayerVote(1, "Nova", 2, "Echo");
            yield return new WaitForSeconds(1.5f);
            socket.SimulatePlayerVote(2, "Echo", 4, "Helix");
            yield return new WaitForSeconds(1.5f);
            socket.SimulatePlayerVote(3, "Pulse", 2, "Echo");
            yield return new WaitForSeconds(1.5f);
            socket.SimulatePlayerVote(4, "Helix", 5, "Drift");
            yield return new WaitForSeconds(1.5f);
            socket.SimulatePlayerVote(5, "Drift", 2, "Echo");
            yield return new WaitForSeconds(2f);

            // Elimination
            socket.SimulateElimination(2, "Echo", "spy", "pear");
            yield return new WaitForSeconds(3f);

            // Game end
            socket.SimulateGameEnd("civilian");

            isRunning = false;
        }
    }
}
