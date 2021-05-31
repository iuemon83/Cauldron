using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardSetConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this CardSetCondition cardTypeCondition,
            Card owner, EffectEventArgs effectEventArgs, Card checkCard)
        {
            return await cardTypeCondition.IsMatch(owner, effectEventArgs, checkCard.CardSetName);
        }

        public static async ValueTask<bool> IsMatch(this CardSetCondition cardTypeCondition,
            Card owner, EffectEventArgs effectEventArgs, CardDef checkCarddef)
        {
            return await cardTypeCondition.IsMatch(owner, effectEventArgs, checkCarddef.CardSetName);
        }

        public static async ValueTask<bool> IsMatch(this CardSetCondition cardTypeCondition,
            Card owner, EffectEventArgs effectEventArgs, string checkValue)
        {
            return cardTypeCondition.Type switch
            {
                CardSetCondition.ConditionType.This => owner.CardSetName == checkValue,
                CardSetCondition.ConditionType.Other => owner.CardSetName != checkValue
                    && cardTypeCondition.ValueCondition != null
                    && await cardTypeCondition.ValueCondition.IsMatch(owner, effectEventArgs, checkValue),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
