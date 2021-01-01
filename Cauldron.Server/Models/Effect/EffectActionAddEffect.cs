using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// 効果を付与する
    /// </summary>
    public record EffectActionAddEffect(Choice CardsChoice, IEnumerable<CardEffect> EffectToAdd) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var targets = args.GameMaster.ChoiceCards(effectOwnerCard, this.CardsChoice, args).CardList;

            var done = false;
            foreach (var card in targets)
            {
                args.GameMaster.AddEffect(card, EffectToAdd);
            }

            return (done, args);
        }
    }
}
