using System;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// ダメージの修整
    /// </summary>
    public class EffectActionModifyDamage : IEffectAction
    {
        public Choice Choice { get; set; }

        public ValueModifier Value { get; set; }

        public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs args)
        {
            var done = false;

            var result = args;

            if (args.BattleContext != null)
            {
                result = args with
                {
                    BattleContext = args.BattleContext with
                    {
                        Value = Math.Max(0, this.Value.Modify(args.BattleContext.Value))
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
                        Value = Math.Max(0, this.Value.Modify(args.DamageContext.Value))
                    }
                };

                done = true;
            }

            return (done, result);
        }
    }
}
