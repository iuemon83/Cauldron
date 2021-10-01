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
            if (args.SourceCard == null)
            {
                return false;
            }

            var matchSource = false;
            foreach (var con in _this.OrCardConditions.Where(c => c != null))
            {
                var isMatched = await con.IsMatch(args.SourceCard, ownerCard, args);
                if (isMatched)
                {
                    matchSource = true;
                }
            }

            var opponentId = args.GameMaster.GetOpponent(ownerCard.OwnerId).Id;

            var (fromSuccess, fromZone) = _this.From.TryGetZone(
                ownerCard.OwnerId,
                opponentId,
                args.SourceCard.OwnerId);
            var (toSuccess, toZone) = _this.To.TryGetZone(
                ownerCard.OwnerId,
                opponentId,
                args.SourceCard.OwnerId);

            return matchSource
                && fromSuccess
                && toSuccess
                && fromZone == args.MoveCardContext.From
                && toZone == args.MoveCardContext.To;
        }
    }
}
