using Cauldron.Server.Models.Effect.Value;

namespace Cauldron.Server.Models.Effect
{
    public record EffectActionSetVariable(string Name, NumValue NumValue) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            if (this.NumValue != null)
            {
                var value = this.NumValue.Calculate(effectOwnerCard, args);
                args.GameMaster.SetVariable(effectOwnerCard.Id, this.Name, value);
            }

            return (true, args);
        }
    }
}
