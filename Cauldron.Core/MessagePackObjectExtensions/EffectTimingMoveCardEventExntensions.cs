using Cauldron.Core.Entities.Effect;

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
                && (_this.From == null
                    || await _this.From.IsMatch(ownerCard, args, args.MoveCardContext.From))
                && (_this.To == null
                    || await _this.To.IsMatch(ownerCard, args, args.MoveCardContext.To));
        }
    }
}
