namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardTypeConditionExtensions
    {
        public static bool IsMatch(this CardTypeCondition cardTypeCondition, CardType checkValue)
        {
            var result = cardTypeCondition.Value.Contains(checkValue);
            return cardTypeCondition.Not ? !result : result;
        }
    }
}
