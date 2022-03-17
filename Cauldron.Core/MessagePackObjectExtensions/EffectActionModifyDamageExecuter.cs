using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyDamageExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyDamage _this;

        public EffectActionModifyDamageExecuter(EffectActionModifyDamage _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
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
