namespace Cauldron.Server.Models.Effect
{
    public record EffectActionModifyPlayer(Choice Choice, PlayerModifier PlayerModifier) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs args)
        {
            var targets = args.GameMaster.ChoiceCards(ownerCard, this.Choice, args).PlayerList;

            var done = false;
            foreach (var player in targets)
            {
                args.GameMaster.ModifyPlayer(new(player.Id, this.PlayerModifier));

                done = true;
            }

            return (done, args);
        }
    }
}
