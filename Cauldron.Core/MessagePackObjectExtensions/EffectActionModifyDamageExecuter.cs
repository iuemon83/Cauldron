using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
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

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
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
                        // < 0 の判定はしない。
                        // damagebeforeイベント完了後に< 0なら0に置換する
                        // 途中で0のときに軽減効果が発動しないようになってしまうため
                        Value = value
                        //Value = Math.Max(0, value)
                    }
                };

                done = true;
            }

            return (done, result);
        }
    }
}
