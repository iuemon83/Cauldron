using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ReadyGameReply
    {
        public enum CodeValue
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

        public CodeValue Code { get; }

        public GameContext GameContext { get; }
        public MoveCardNotifyMessage MoveCardNotify { get; }
        public AddCardNotifyMessage AddCardNotify { get; }
        public ModifyCardNotifyMessage ModifyCardNotify { get; }
        public ModifyPlayerNotifyMessage ModifyPlayerNotify { get; }
        public DamageNotifyMessage DamageNotify { get; }

        public ReadyGameReply(
            CodeValue Code,
            GameContext GameContext,
            MoveCardNotifyMessage MoveCardNotify,
            AddCardNotifyMessage AddCardNotify,
            ModifyCardNotifyMessage ModifyCardNotify,
            ModifyPlayerNotifyMessage ModifyPlayerNotify,
            DamageNotifyMessage DamageNotify
            )
        {
            this.Code = Code;
            this.GameContext = GameContext;
            this.MoveCardNotify = MoveCardNotify;
            this.AddCardNotify = AddCardNotify;
            this.ModifyCardNotify = ModifyCardNotify;
            this.ModifyPlayerNotify = ModifyPlayerNotify;
            this.DamageNotify = DamageNotify;
        }
    }
}
