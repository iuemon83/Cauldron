namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果で、カードを破壊する処理
    /// </summary>
    public class EffectActionDestroyCard
    {
        public Choice Choice { get; set; }

        public bool Execute(Card ownerCard, EffectEventArgs args)
        {
            var choiceResult = args.GameMaster.ChoiceCards(ownerCard, this.Choice, args);

            var done = false;
            foreach (var card in choiceResult.CardList)
            {
                args.GameMaster.DestroyCard(card);

                done = true;
            }

            return done;
        }
    }
}
