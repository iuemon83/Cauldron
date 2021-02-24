using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ReadyGameReply
    {
        public enum TypeCode
        {
            Ready,
            StartGame,
            GameOver,
            StartTurn,
            AddCard,
            MoveCard,
            ModifyCard,
            ModifyPlayer,
            Damage,
        }

        public ReadyGameReply.TypeCode Code { get; set; }

        public GameContext GameContext { get; set; }
        public MoveCardNotifyMessage MoveCardNotify { get; set; }
        public AddCardNotifyMessage AddCardNotify { get; set; }
        public ModifyCardNotifyMessage ModifyCardNotify { get; set; }
        public ModifyPlayerNotifyMessage ModifyPlayerNotify { get; set; }
        public DamageNotifyMessage DamageNotify { get; set; }
    }
}
