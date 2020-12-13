namespace Cauldron.Server.Models.Effect
{
    public class EffectActionDamage : IEffectAction
    {
        public int Value { get; set; }
        public Choice Choice { get; set; }

        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = args.GameMaster.ChoiceCards(effectOwnerCard, this.Choice, args);

            var done = false;

            foreach (var playerId in choiceResult.PlayerList)
            {
                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: this.Value,
                    GuardPlayer: playerId
                    );

                args.GameMaster.HitPlayer(damageContext);

                done = true;
            }

            foreach (var card in choiceResult.CardList)
            {
                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: this.Value,
                    GuardCard: card
                    );
                args.GameMaster.HitCreature(damageContext);

                done = true;
            }

            return (done, args);
        }
    }
}
