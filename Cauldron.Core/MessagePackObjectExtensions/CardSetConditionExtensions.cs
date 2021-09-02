using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardSetConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this CardSetCondition _this,
            Card owner, EffectEventArgs effectEventArgs, Card checkCard)
        {
            return await _this.IsMatch(owner, effectEventArgs, checkCard.CardSetName);
        }

        public static async ValueTask<bool> IsMatch(this CardSetCondition _this,
            Card owner, EffectEventArgs effectEventArgs, CardDef checkCarddef)
        {
            return await _this.IsMatch(owner, effectEventArgs, checkCarddef.CardSetName);
        }

        public static async ValueTask<bool> IsMatch(this CardSetCondition _this,
            Card owner, EffectEventArgs effectEventArgs, string checkValue)
        {
            return _this.Type switch
            {
                CardSetCondition.TypeValue.This => owner.CardSetName == checkValue,
                CardSetCondition.TypeValue.Other => owner.CardSetName != checkValue
                    && _this.ValueCondition != null
                    && await _this.ValueCondition.IsMatch(owner, effectEventArgs, checkValue),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
