namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果で、カードを破壊する処理
    /// </summary>
    public record EffectActionDestroyCard(Choice Choice, string Name = null) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = args.GameMaster.ChoiceCards(effectOwnerCard, this.Choice, args);

            var done = false;
            foreach (var card in choiceResult.CardList)
            {
                args.GameMaster.DestroyCard(card);

                done = true;
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                var context = new ActionContext(new ActionDestroyCardContext(choiceResult.CardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, this.Name, context);
            }

            return (done, args);
        }
    }
}
