using System;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// ダメージの修整
    /// </summary>
    public class EffectActionModifyDamage
    {
        public Choice Choice { get; set; }

        public ValueModifier Value { get; set; }

        public bool Execute(EffectEventArgs args)
        {
            var done = false;

            if (args.BattleContext != null)
            {
                args.BattleContext.Value = Math.Max(0, this.Value.Modify(args.BattleContext.Value));

                done = true;
            }

            if (args.DamageContext != null)
            {
                args.DamageContext.Value = Math.Max(0, this.Value.Modify(args.DamageContext.Value));

                done = true;
            }

            return done;
        }
    }
}
