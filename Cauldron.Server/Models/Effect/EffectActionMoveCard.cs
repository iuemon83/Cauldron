namespace Cauldron.Server.Models.Effect
{
    public record EffectActionMoveCard(Choice CardsChoice, ZonePrettyName To, string Name = null) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var targets = args.GameMaster.ChoiceCards(effectOwnerCard, this.CardsChoice, args).CardList;

            var done = false;
            foreach (var card in targets)
            {
                var toZone = args.GameMaster.ConvertZone(effectOwnerCard.OwnerId, this.To);
                args.GameMaster.MoveCard(card.Id, new(card.Zone, toZone));

                done = true;
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                var context = new ActionContext(ActionMoveCardContext: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, this.Name, context);
            }

            return (done, args);
        }
    }
}
