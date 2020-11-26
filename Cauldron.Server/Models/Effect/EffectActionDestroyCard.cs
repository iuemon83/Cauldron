namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果で、カードを破壊する処理
    /// </summary>
    public class EffectActionDestroyCard
    {
        public Choice Choice { get; set; }

        public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource)
        {
            var choiceResult = gameMaster.ChoiceCards(ownerCard, this.Choice, eventSource);

            foreach (var card in choiceResult.CardList)
            {
                gameMaster.DestroyCard(card);
            }
        }

        public void Execute(Card ownerCard, EffectEventArgs args) => this.Execute(args.GameMaster, ownerCard, args.Source);
    }
}
