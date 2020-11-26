namespace Cauldron.Server.Models.Effect
{
    public class EffectActionModifyCard
    {
        public Choice Choice { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }

        public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource)
        {
            var targets = gameMaster.ChoiceCards(ownerCard, this.Choice, eventSource).CardList;

            foreach (var card in targets)
            {
                gameMaster.Buff(card, this.Power, this.Toughness);
            }
        }

        public void Execute(Card ownerCard, EffectEventArgs args) => this.Execute(args.GameMaster, ownerCard, args.Source);
    }
}
