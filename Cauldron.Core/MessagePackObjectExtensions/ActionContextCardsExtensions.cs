using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsExtensions
    {
        public static IEnumerable<Card> GetCards(this ActionContextCards _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this?.AddCard != null)
            {
                return _this.AddCard.GetRsult(effectOwnerCard, eventArgs);
            }
            if (_this?.AddEffect != null)
            {
                return _this.AddEffect.GetRsult(effectOwnerCard, eventArgs);
            }
            if (_this?.Damage != null)
            {
                return _this.Damage.GetRsult(effectOwnerCard, eventArgs);
            }
            if (_this?.DestroyCard != null)
            {
                return _this.DestroyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            if (_this?.DrawCard != null)
            {
                return _this.DrawCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.ExcludeCard != null)
            {
                return _this.ExcludeCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.ModifyCard != null)
            {
                return _this.ModifyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.ModifyCounter != null)
            {
                return _this.ModifyCounter.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.MoveCard != null)
            {
                return _this.MoveCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.Choice != null)
            {
                return _this.Choice.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return Array.Empty<Card>();
            }
        }
    }
}
