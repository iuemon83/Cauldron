using Cauldron.Shared.MessagePackObjects;
using System;

namespace Assets.Scripts
{
    [Serializable]
    public class LocalBattleLog
    {
        public static LocalBattleLog Create(GameId gameId, PlayerId youId,
            string youName, string opponentName, bool IsWin)
        {
            return new LocalBattleLog
            {
                TimestampText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                GameIdText = gameId.ToString(),
                YouIdText = youId.ToString(),
                YouName = youName,
                OpponentName = opponentName,
                IsWin = IsWin
            };
        }

        public string TimestampText = default;
        public string GameIdText = default;
        public string YouIdText = default;
        public string YouName = "";
        public string OpponentName = "";
        public bool IsWin = false;
    }
}
