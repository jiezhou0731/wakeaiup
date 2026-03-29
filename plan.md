# Plan: Add "Who Is The Spy" Game Coordinator

## Context
The wakeaiup project is an evolutionary AI framework where AI models play games, accumulate memories, and evolve. Currently the repo contains only a static site (index.html, README, diary entries). We need to build a server that coordinates a "Who Is The Spy" (谁是卧底) game played by 6 AI vessels, using Ollama for AI decision-making, and a Unity client that visualizes the game via Socket.IO.

---

## File Structure

```
wakeaiup/
├── server/
│   ├── package.json
│   ├── index.js                    # Entry point: HTTP + Socket.IO server
│   ├── game/
│   │   ├── SpyGameCoordinator.js   # Game state machine & orchestration
│   │   ├── Vessel.js               # AI player (vessel) class
│   │   └── WordPairs.js            # Word pair database (civilian/spy words)
│   ├── ai/
│   │   └── OllamaService.js        # Ollama HTTP client with structured I/O
│   └── config.js                   # Server/game configuration constants
├── unity/
│   └── Assets/
│       └── Scripts/
│           ├── Network/
│           │   └── SocketManager.cs      # Socket.IO connection & event handling
│           ├── Game/
│           │   ├── SpyGameManager.cs     # Client-side game state & UI orchestration
│           │   ├── PlayerSlot.cs         # Per-player visual representation
│           │   └── GameUIController.cs   # Speech bubbles, vote display, announcements
│           └── Data/
│               └── GameMessages.cs       # Shared message data classes (deserialize socket payloads)
└── (existing files: index.html, README.md, diary/, etc.)
```

---

## Server Components

### 1. `server/index.js` — Entry Point
- Create Express HTTP server + Socket.IO
- On Unity client connect: register as spectator
- Expose a `start_game` socket event (or auto-start on connect)
- Instantiate `SpyGameCoordinator` and run the game loop

### 2. `server/game/SpyGameCoordinator.js` — Game State Machine

**State Machine:**
```
WAITING → ROLE_ASSIGNMENT → DESCRIPTION_ROUND → VOTING → ELIMINATION → CHECK_WIN → (loop or END)
```

**Responsibilities:**
- Create 6 `Vessel` instances with unique names/IDs
- Pick a random word pair from `WordPairs.js`
- Assign roles: 1 spy + 5 civilians (configurable)
- **Push perception events** to all alive vessels as things happen
- Run game loop through stages:
  1. **ROLE_ASSIGNMENT**: Assign words, emit `game_start` to Unity, push `{ type: "game_start" }` to all vessels
  2. **DESCRIPTION_ROUND**: For each alive player in order:
     - Push `{ type: "stage_change" }` to all vessels
     - Call `OllamaService.getDescription(vessel, gameState)` — this reads & flushes vessel's perception
     - Append `response.memory_to_store` to vessel's `shortTermMemory`
     - Push `{ type: "player_spoke", player, text }` to all **other** alive vessels
     - Emit `player_speak` to Unity
  3. **VOTING**: For each alive player:
     - Push `{ type: "stage_change" }` to all vessels
     - Call `OllamaService.getVote(vessel, gameState)` — reads & flushes perception
     - Append `response.memory_to_store` to vessel's `shortTermMemory`
     - Push `{ type: "player_voted" }` to all other alive vessels
     - Emit `player_vote` to Unity
  4. **ELIMINATION**: Tally votes, eliminate player with most votes (random tiebreak), push `{ type: "player_eliminated" }` to all alive vessels, emit `player_eliminated` to Unity
  5. **CHECK_WIN**: If spy eliminated → civilians win. If civilians ≤ spy count + 1 → spy wins. Otherwise → next round
  6. **END**: Emit `game_end` with winner

**Key method signatures:**
```js
class SpyGameCoordinator {
  constructor(io, ollamaService, config)
  async startGame()
  async runDescriptionRound(roundNumber)
  async runVotingRound(roundNumber)
  eliminatePlayer(votes)
  checkWinCondition() → { finished: bool, winner: 'spy'|'civilian' }
  broadcastPerception(event, excludeVessel?)  // push perception to all alive vessels (optionally skip one)
}
```

### 3. `server/game/Vessel.js` — AI Player with Memory & Perception

Each vessel owns two layers of context that travel with it across Ollama calls:

```js
class Vessel {
  constructor(id, name)

  // --- Identity ---
  id            // unique identifier (0-5)
  name          // display name (e.g., "Cypher", "Vessel-2", ...)
  role          // 'spy' | 'civilian'
  word          // the secret word assigned
  isAlive       // elimination state

  // --- Memory layers ---
  shortTermMemory   // array of strings — persists within current game, Ollama decides what to store here
  perceptionBuffer  // array of objects — ephemeral, accumulates since last Ollama call, flushed after each call

  // --- Methods ---
  pushPerception(event)           // append to perceptionBuffer
  flushPerception()               // return perceptionBuffer contents & clear it
  addShortTermMemory(entries)     // append what Ollama returned as "memory_to_store"
  getFullContext()                // return { shortTermMemory, perceptionBuffer }
  reset()                         // clear both layers (called at game start)
}
```

**Memory layer details:**

| Layer | Lifespan | Who writes | Sent to Ollama? |
|---|---|---|---|
| `shortTermMemory` | Current game | Ollama (via `memory_to_store` in response) | Yes — full array |
| `perceptionBuffer` | Between two Ollama calls | `SpyGameCoordinator` | Yes — then flushed |

- **`shortTermMemory`** is the vessel's own running notes. Ollama decides what to remember after each interaction. It accumulates throughout the game and is sent in full on every call. Reset between games.
- **`perceptionBuffer`** is what happened in the world since this vessel last talked to Ollama. The coordinator pushes events here. It is sent once and then cleared.

**Perception events** — the coordinator pushes these to every alive vessel's `perceptionBuffer` as they happen:

```js
// Examples of perception events pushed between Ollama calls:
{ type: "stage_change",      detail: "Round 2 — Description Phase" }
{ type: "player_spoke",      player: "Vessel-3", text: "It has strings and makes music" }
{ type: "player_voted",      voter: "Vessel-2", target: "Vessel-5" }
{ type: "player_eliminated", player: "Vessel-4", role: "civilian", votes: 3 }
{ type: "vote_result",       summary: "Vessel-4 eliminated with 3 votes" }
{ type: "round_end",         round: 1, alive: ["Cypher","Vessel-2","Vessel-3","Vessel-5"] }
```

This means each vessel only "sees" what happened since it last spoke/voted — just like a real player paying attention between turns.

### 4. `server/game/WordPairs.js` — Word Database
Array of `{ civilian: "...", spy: "..." }` pairs. Examples:
- { civilian: "apple", spy: "pear" }
- { civilian: "ocean", spy: "lake" }
- { civilian: "guitar", spy: "violin" }
- ~20+ pairs to start

### 5. `server/ai/OllamaService.js` — Ollama Integration

**Endpoint:** `POST http://localhost:11434/api/generate` (or `/api/chat`)

**Structured prompt for DESCRIPTION phase:**
```json
{
  "model": "llama3",
  "prompt": "<structured prompt below>",
  "format": "json",
  "stream": false
}
```

**Description prompt structure sent to Ollama:**
```json
{
  "task": "describe",
  "guidance": "You are playing 'Who Is The Spy'. You received a secret word. Describe it without saying it directly. Be subtle — if you're too obvious, others will know your word. If you're the spy, try to blend in by listening to others' descriptions. Keep your description to 1-2 sentences.",

  "identity": {
    "your_name": "Cypher",
    "your_word": "apple",
    "your_role_hint": "You do not know if you are the spy or a civilian."
  },

  "game_state": {
    "round": 2,
    "phase": "describe",
    "alive_players": ["Cypher", "Vessel-2", "Vessel-3", "Vessel-5"]
  },

  "perception": [
    { "type": "stage_change",   "detail": "Round 2 — Description Phase" },
    { "type": "player_spoke",   "player": "Vessel-2", "text": "It grows on trees and is juicy" },
    { "type": "player_spoke",   "player": "Vessel-3", "text": "You can find it in an orchard" }
  ],

  "short_term_memory": [
    "Vessel-4 described something about 'making sound' — felt off compared to others.",
    "I described it as 'round and you can hold it' in Round 1. Nobody reacted suspiciously."
  ]
}
```

**Expected JSON response from Ollama (describe):**
```json
{
  "description": "It's something refreshing that comes in different colors",
  "reasoning": "Others mentioned trees and orchard — I'll stay consistent but vague",
  "memory_to_store": [
    "Vessel-2 and Vessel-3 both referenced trees/orchards — they seem aligned.",
    "I went with 'refreshing and colorful' to stay safe."
  ]
}
```

**Voting prompt structure sent to Ollama:**
```json
{
  "task": "vote",
  "guidance": "Vote for the player you think is the spy. Analyze everyone's descriptions — the spy has a similar but different word, so their descriptions might be slightly off. You cannot vote for yourself. Respond with the exact player name.",

  "identity": {
    "your_name": "Cypher",
    "your_word": "apple",
    "your_role_hint": "You do not know if you are the spy or a civilian."
  },

  "game_state": {
    "round": 2,
    "phase": "vote",
    "alive_players": ["Cypher", "Vessel-2", "Vessel-3", "Vessel-5"]
  },

  "perception": [
    { "type": "player_spoke", "player": "Vessel-5", "text": "People enjoy it in summer" },
    { "type": "stage_change", "detail": "Round 2 — Voting Phase" }
  ],

  "short_term_memory": [
    "Vessel-2 and Vessel-3 both referenced trees/orchards — they seem aligned.",
    "I went with 'refreshing and colorful' to stay safe.",
    "Vessel-4 was eliminated Round 1 — turned out civilian."
  ]
}
```

**Expected JSON response from Ollama (vote):**
```json
{
  "vote": "Vessel-5",
  "reasoning": "Vessel-5's 'enjoy it in summer' is vague and could apply to many things — feels like a spy hedging",
  "memory_to_store": [
    "I voted for Vessel-5 because their description felt generic and evasive."
  ]
}
```

**Key: `memory_to_store`** — Ollama decides what the vessel should remember. These strings get appended to the vessel's `shortTermMemory` and will be included in all future Ollama calls for this vessel during this game. This lets the AI build up its own running internal monologue / notes.

**OllamaService methods:**
```js
class OllamaService {
  constructor(baseUrl, model)

  async getDescription(vessel, gameState) → { description, reasoning, memory_to_store }
  async getVote(vessel, gameState)        → { vote, reasoning, memory_to_store }

  // Builds the full prompt by assembling:
  //   identity + game_state + vessel.perceptionBuffer + vessel.shortTermMemory
  buildPrompt(task, vessel, gameState) → structured object
}
```

**Flow per Ollama call:**
1. `buildPrompt()` reads `vessel.perceptionBuffer` and `vessel.shortTermMemory`
2. Send to Ollama, get response
3. `vessel.flushPerception()` — clear the buffer (already consumed)
4. `vessel.addShortTermMemory(response.memory_to_store)` — store what Ollama wants to remember

### 6. `server/config.js`
```js
module.exports = {
  PORT: 3000,
  OLLAMA_URL: 'http://localhost:11434',
  OLLAMA_MODEL: 'llama3',
  NUM_PLAYERS: 6,
  NUM_SPIES: 1,
  VESSEL_NAMES: ['Cypher', 'Vessel-2', 'Vessel-3', 'Vessel-4', 'Vessel-5', 'Vessel-6'],
  SPEAK_DELAY_MS: 2000,   // delay between players speaking (for Unity animation)
  VOTE_DELAY_MS: 1000,
};
```

---

## Socket Event Protocol (Server → Unity)

| Event Name | Payload | When |
|---|---|---|
| `game_start` | `{ players: [{id, name}], round: 1 }` | Game begins |
| `round_start` | `{ round: number, phase: "describe", alive_players: [ids] }` | Each new round |
| `player_speak` | `{ player_id, player_name, text, round }` | A vessel gives description |
| `vote_phase_start` | `{ round }` | Voting begins |
| `player_vote` | `{ voter_id, voter_name, target_id, target_name, round }` | A vessel casts vote |
| `player_eliminated` | `{ player_id, player_name, role, word, votes_received }` | Someone is out |
| `game_end` | `{ winner: "spy"|"civilian", rounds_played, spy_id, spy_name, civilian_word, spy_word, summary }` | Game over |
| `game_error` | `{ message }` | Error occurred |

**Unity → Server:**

| Event Name | Payload | When |
|---|---|---|
| `request_start` | `{}` | Unity client requests game start |
| `request_restart` | `{}` | Request new game |

---

## Unity Client Components

### 1. `SocketManager.cs`
- Uses `socket.io-client` for Unity (e.g., `SocketIOUnity` NuGet package or `best-socket-io`)
- Connects to `ws://localhost:3000`
- Registers listeners for all server events
- Dispatches C# events to `SpyGameManager`

### 2. `GameMessages.cs` — Data Classes
```csharp
[Serializable] public class GameStartMessage { public PlayerInfo[] players; public int round; }
[Serializable] public class PlayerInfo { public int id; public string name; }
[Serializable] public class PlayerSpeakMessage { public int player_id; public string player_name; public string text; public int round; }
[Serializable] public class PlayerVoteMessage { public int voter_id; public string voter_name; public int target_id; public string target_name; public int round; }
[Serializable] public class PlayerEliminatedMessage { public int player_id; public string player_name; public string role; public string word; public int votes_received; }
[Serializable] public class GameEndMessage { public string winner; public int rounds_played; public int spy_id; public string spy_name; public string civilian_word; public string spy_word; }
```

### 3. `SpyGameManager.cs`
- Singleton MonoBehaviour
- Manages 6 `PlayerSlot` references (arranged in a circle/around a table in the scene)
- On `game_start`: initialize player slots with names
- On `player_speak`: highlight active player, show speech bubble with text
- On `player_vote`: show vote indicator line/arrow from voter to target
- On `player_eliminated`: grey out / fade / X-mark the eliminated player slot
- On `game_end`: show result screen (winner, reveal roles)

### 4. `PlayerSlot.cs`
- References: avatar/sprite, name label (TextMeshPro), speech bubble panel, vote indicator, eliminated overlay
- Methods: `SetName()`, `ShowSpeech(text)`, `HideSpeech()`, `ShowVote(targetSlot)`, `Eliminate()`, `Reset()`

### 5. `GameUIController.cs`
- Round counter display
- Phase indicator ("Description Phase", "Voting Phase")
- Start/Restart button
- Game result panel
- Announcement banner (e.g., "Vessel-3 has been eliminated! They were a Civilian.")

---

## Implementation Order

### Step 1: Server scaffolding
- Create `server/package.json` with dependencies: `express`, `socket.io`, `uuid`
- Create `server/config.js`
- Create `server/index.js` with basic Express + Socket.IO setup

### Step 2: Game core classes
- Create `server/game/WordPairs.js`
- Create `server/game/Vessel.js`
- Create `server/game/SpyGameCoordinator.js` (full state machine)

### Step 3: Ollama integration
- Create `server/ai/OllamaService.js`
- Wire into SpyGameCoordinator for description and voting phases

### Step 4: Unity client scripts
- Create `unity/Assets/Scripts/Data/GameMessages.cs`
- Create `unity/Assets/Scripts/Network/SocketManager.cs`
- Create `unity/Assets/Scripts/Game/PlayerSlot.cs`
- Create `unity/Assets/Scripts/Game/GameUIController.cs`
- Create `unity/Assets/Scripts/Game/SpyGameManager.cs`

---

## Verification

1. **Server standalone test**: Run `node server/index.js`, observe console logs showing game progression through all stages
2. **Ollama test**: Ensure Ollama is running with a model, verify structured JSON responses parse correctly
3. **Memory flow test**: Verify vessel `shortTermMemory` accumulates across rounds (log it), and `perceptionBuffer` flushes correctly after each Ollama call
4. **Socket test**: Connect a socket.io test client, verify all events are received in correct order
5. **Unity integration**: Open Unity project, connect to running server, verify visual game flow
