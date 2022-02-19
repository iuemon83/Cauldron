using Cauldron.Core.Entities.Effect;
using Cauldron.Core.MessagePackObjectExtensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="ownerCard"></param>
        /// <param name="effectEventArgs"></param>
        /// <returns></returns>
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectAction _this,
            Card ownerCard, EffectEventArgs effectEventArgs)
        {
            //TODO この順番もけっこう重要
            var actions = new IEffectActionExecuter?[]
            {
                _this.AddCard == null ? null: new EffectActionAddCardExecuter(_this.AddCard),
                _this.AddEffect == null ? null: new EffectActionAddEffectExecuter(_this.AddEffect),
                _this.Damage == null ? null: new EffectActionDamageExecuter(_this.Damage),
                _this.DestroyCard == null ? null: new EffectActionDestroyCardExecuter(_this.DestroyCard),
                _this.DrawCard == null ? null: new EffectActionDrawCardExecuter(_this.DrawCard),
                _this.ExcludeCard == null ? null: new EffectActionExcludeCardExecuter(_this.ExcludeCard),
                _this.ModifyCard == null ? null: new EffectActionModifyCardExecuter(_this.ModifyCard),
                _this.ModifyCounter == null ? null: new EffectActionModifyCounterExecuter(_this.ModifyCounter),
                _this.ModifyDamage == null ? null: new EffectActionModifyDamageExecuter(_this.ModifyDamage),
                _this.ModifyPlayer == null ? null: new EffectActionModifyPlayerExecuter(_this.ModifyPlayer),
                _this.MoveCard == null ? null: new EffectActionMoveCardExecuter(_this.MoveCard),
                _this.SetVariable == null ? null: new EffectActionSetVariableExecuter(_this.SetVariable),
                _this.Win == null ? null: new EffectActionWinExecuter(_this.Win),
                _this.ReserveEffect == null ? null: new EffectActionReserveEffectExecuter(_this.ReserveEffect),
            }
            .OfType<IEffectActionExecuter>()
            .ToArray();

            var result = effectEventArgs;
            var done = false;
            foreach (var action in actions)
            {
                var (done2, result2) = await action.Execute(ownerCard, result);

                done = done || done2;
                result = result2;
            }

            return (done, result);
        }
    }
}
