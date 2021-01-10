namespace Cauldron.Server.Models.Effect
{
    public record EffectActionModifyCard(int Power, int Toughness, Choice Choice) : IEffectAction
    {
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
