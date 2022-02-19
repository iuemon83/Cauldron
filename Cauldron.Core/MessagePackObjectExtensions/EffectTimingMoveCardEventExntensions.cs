using Cauldron.Core.Entities.Effect;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingMoveCardEventExntensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingMoveCardEvent _this,
            Card ownerCard, EffectEventArgs args)
        {
            if (args.SourceCard == null || args.MoveCardContext == null)
            {
                return false;
            }

            var matchSource = false;
            foreach (var con in _this.OrCardConditions.Where(c => c != null))
            {
                var isMatched = await con.IsMatch(ownerCard, args, args.SourceCard);
                if (isMatched)
                {
                    matchSource = true;
                }
            }

            var opponentId = args.GameMaster.GetOpponent(ownerCard.OwnerId).Id;

            return matchSource
                && IsMatchZone(_this.From, args.MoveCardContext.From,
                    ownerCard.OwnerId,
                    opponentId,
                    args.SourceCard.OwnerId)
                && IsMatchZone(_this.To, args.MoveCardContext.To,
                    ownerCard.OwnerId,
                    opponentId,
                    args.SourceCard.OwnerId);


            static bool IsMatchZone(ZonePrettyName zonePrettyName, Zone other, PlayerId effectOwnerId, PlayerId opponentId, PlayerId cardOwnerId)
            {
                if (zonePrettyName == ZonePrettyName.None)
                {
                    return true;
                }

                var (success, zone) = zonePrettyName.TryGetZone(
                    effectOwnerId,
                    opponentId,
                    cardOwnerId);

                return success && zone == other;
            }
        }
    }
}
