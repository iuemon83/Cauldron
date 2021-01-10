using Cauldron.Server.Models.Effect.Value;
using System;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// ダメージの修整
    /// </summary>
    public record EffectActionModifyDamage(NumValueModifier Value, Choice Choice) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var done = false;

            var result = args;

            if (args.BattleContext != null)
            {
                result = args with
                {
                    BattleContext = args.BattleContext with
                    {
                        Value = Math.Max(0, this.Value.Modify(effectOwnerCard, args, args.BattleContext.Value))
                    }
                };

                done = true;
            }

            if (args.DamageContext != null)
            {
                result = args with
                {
                    DamageContext = args.DamageContext with
                    {
                        Value = Math.Max(0, this.Value.Modify(effectOwnerCard, args, args.DamageContext.Value))
                    }
                };

                done = true;
            }

            return (done, result);
        }
    }
}
