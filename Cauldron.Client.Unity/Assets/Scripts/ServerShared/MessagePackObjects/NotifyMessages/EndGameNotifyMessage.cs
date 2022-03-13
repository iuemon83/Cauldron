#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// ゲーム終了した理由
    /// </summary>
    public enum EndGameReason
    {
        HpIsZero,
        CardEffect,
        Surrender
    }

    [MessagePackObject(true)]
    public class EndGameNotifyMessage
    {
        public PlayerId WinnerPlayerId { get; }
        public EndGameReason EndGameReason { get; }
        public string? CardName { get; }

        public EndGameNotifyMessage(PlayerId WinnerPlayerId, EndGameReason EndGameReason, string? CardName)
        {
            this.WinnerPlayerId = WinnerPlayerId;
            this.EndGameReason = EndGameReason;
            this.CardName = CardName;
        }
    }
}
