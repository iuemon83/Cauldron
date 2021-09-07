using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionModifyDamageExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyDamage _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var done = false;

            var result = args;

            if (args.DamageContext != null)
            {
                var value = await _this.Value.Modify(effectOwnerCard, args, args.DamageContext.Value);
                result = args with
                {
                    DamageContext = args.DamageContext with
                    {
                        Value = Math.Max(0, value)
                    }
                };

                done = true;
            }

            return (done, result);
        }
    }
}
