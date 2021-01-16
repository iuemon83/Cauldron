using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード破壊アクションのコンテキスト（カード型）
    /// </summary>
    public record ActionContextCardsOfAddEffect(string ActionName, ActionContextCardsOfAddEffect.ValueType Type)
    {
        public enum ValueType
        {
            TargetCards
        }

        public IEnumerable<Card> GetRsult(Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, this.ActionName, out var value)
                ? value?.ActionAddEffectContext?.GetCards(this.Type)
                : System.Array.Empty<Card>();
        }
    }
}
