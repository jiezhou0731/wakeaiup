using System;

namespace WakeAIUp.Data
{
    [Serializable]
    public class PlayerInfo
    {
        public int id;
        public string name;
    }

    [Serializable]
    public class GameStartMessage
    {
        public PlayerInfo[] players;
        public int round;
    }

    [Serializable]
    public class RoundStartMessage
    {
        public int round;
        public string phase;
        public int[] alive_players;
    }

    [Serializable]
    public class PlayerSpeakMessage
    {
        public int player_id;
        public string player_name;
        public string text;
        public int round;
    }

    [Serializable]
    public class PlayerVoteMessage
    {
        public int voter_id;
        public string voter_name;
        public int target_id;
        public string target_name;
        public int round;
    }

    [Serializable]
    public class PlayerEliminatedMessage
    {
        public int player_id;
        public string player_name;
        public string role;
        public string word;
        public int votes_received;
    }

    [Serializable]
    public class GameEndMessage
    {
        public string winner;
        public int rounds_played;
        public int spy_id;
        public string spy_name;
        public string civilian_word;
        public string spy_word;
        public string summary;
    }

    [Serializable]
    public class GameErrorMessage
    {
        public string message;
    }
}
