namespace Cauldron.Server.Models.Effect
{
    public class EffectActionModifyCard : IEffectAction
    {
        public Choice Choice { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }

        public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs args)
        {
            var targets = args.GameMaster.ChoiceCards(ownerCard, this.Choice, args).CardList;

            var done = false;
            foreach (var card in targets)
            {
                args.GameMaster.Buff(card, this.Power, this.Toughness);

                done = true;
            }

            return (done, args);
        }
    }
}
