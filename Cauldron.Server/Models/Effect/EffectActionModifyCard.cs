using Cauldron.Server.Models.Effect.Value;

namespace Cauldron.Server.Models.Effect
{
    public record EffectActionModifyCard(NumValue Power, NumValue Toughness, Choice Choice) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var targets = effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, this.Choice, effectEventArgs).CardList;

            var done = false;
            var buffPower = this.Power?.Calculate(effectOwnerCard, effectEventArgs) ?? 0;
            var buffToughness = this.Toughness?.Calculate(effectOwnerCard, effectEventArgs) ?? 0;
            foreach (var card in targets)
            {
                effectEventArgs.GameMaster.Buff(card, buffPower, buffToughness);

                done = true;
            }

            return (done, effectEventArgs);
        }
    }
}
