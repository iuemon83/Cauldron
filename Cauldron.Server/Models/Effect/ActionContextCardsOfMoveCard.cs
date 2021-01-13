using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード移動アクションのコンテキスト（カード型）
    /// </summary>
    public record ActionContextCardsOfMoveCard(string ActionName, ActionContextCardsOfMoveCard.ValueType Type)
    {
        public enum ValueType
        {
            Moved
        }

        public IEnumerable<Card> GetRsult(Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, this.ActionName, out var value)
                ? value?.ActionMoveCardContext?.GetCards(this.Type)
                : Array.Empty<Card>();
        }
    }
}
