using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AttackToCreatureRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }
        public CardId AttackCardId { get; }
        public CardId GuardCardId { get; }

        public AttackToCreatureRequest(GameId GameId, PlayerId PlayerId, CardId AttackCardId, CardId GuardCardId)
        {
            this.GameId = GameId;
            this.PlayerId = PlayerId;
            this.AttackCardId = AttackCardId;
            this.GuardCardId = GuardCardId;
        }
    }
}
