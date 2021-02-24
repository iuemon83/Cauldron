using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AttackToPlayerRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }
        public CardId AttackCardId { get; }
        public PlayerId GuardPlayerId { get; }

        public AttackToPlayerRequest(GameId GameId, PlayerId PlayerId, CardId AttackCardId, PlayerId GuardPlayerId)
        {
            this.GameId = GameId;
            this.PlayerId = PlayerId;
            this.AttackCardId = AttackCardId;
            this.GuardPlayerId = GuardPlayerId;
        }
    }
}
