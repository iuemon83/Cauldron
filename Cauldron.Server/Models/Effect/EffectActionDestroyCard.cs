namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果で、カードを破壊する処理
    /// </summary>
    public record EffectActionDestroyCard(Choice Choice) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs args)
        {
            var choiceResult = args.GameMaster.ChoiceCards(ownerCard, this.Choice, args);

            var done = false;
            foreach (var card in choiceResult.CardList)
            {
                args.GameMaster.DestroyCard(card);

                done = true;
            }

            return (done, args);
        }
    }
}
