namespace Cauldron.Server.Models.Effect
{
    public class EffectActionDamage
    {
        public int Value { get; set; }
        public Choice Choice { get; set; }

        public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource)
        {
            var choiceResult = gameMaster.ChoiceCards(ownerCard, this.Choice, eventSource);

            foreach (var playerId in choiceResult.PlayerIdList)
            {
                gameMaster.HitPlayer(playerId, this.Value);
            }

            foreach (var card in choiceResult.CardList)
            {
                gameMaster.HitCreature(card.Id, this.Value);
            }
        }

        public void Execute(Card ownerCard, EffectEventArgs args) => this.Execute(args.GameMaster, ownerCard, args.Source);
    }
}
