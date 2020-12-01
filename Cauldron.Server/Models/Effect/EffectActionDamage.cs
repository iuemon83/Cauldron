namespace Cauldron.Server.Models.Effect
{
    public class EffectActionDamage
    {
        public int Value { get; set; }
        public Choice Choice { get; set; }

        public bool Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = args.GameMaster.ChoiceCards(effectOwnerCard, this.Choice, args);

            var done = false;

            foreach (var playerId in choiceResult.PlayerList)
            {
                var damageContext = new DamageContext()
                {
                    DamageSourceCard = effectOwnerCard,
                    GuardPlayer = playerId,
                    Value = this.Value
                };
                args.GameMaster.HitPlayer(damageContext);

                done = true;
            }

            foreach (var card in choiceResult.CardList)
            {
                var damageContext = new DamageContext()
                {
                    DamageSourceCard = effectOwnerCard,
                    GuardCard = card,
                    Value = this.Value
                };
                args.GameMaster.HitCreature(damageContext);

                done = true;
            }

            return done;
        }
    }
}
