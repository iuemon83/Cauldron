using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード破壊アクションのコンテキスト（カード型）
    /// </summary>
    public record ActionContextCardsOfDestroyCard(string ActionName, ActionContextCardsOfDestroyCard.ValueType Type)
    {
        public enum ValueType
        {
            Destroyed
        }

        public IEnumerable<Card> GetRsult(Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, this.ActionName, out var value)
                ? value?.ActionDestroyCardContext?.GetCards(this.Type)
                : System.Array.Empty<Card>();
        }
    }
}
