using Cauldron.Core.Entities.Effect;
using System;
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
            var actions = new Func<Card, EffectEventArgs, ValueTask<(bool, EffectEventArgs)>?>[]
            {
                (c,e) => _this.AddCard?.Execute(c,e),
                (c,e) => _this.AddEffect?.Execute(c,e),
                (c,e) => _this.Damage?.Execute(c,e),
                (c,e) => _this.DestroyCard?.Execute(c,e),
                (c,e) => _this.DrawCard?.Execute(c,e),
                (c,e) => _this.ExcludeCard?.Execute(c,e),
                (c,e) => _this.ModifyCard?.Execute(c,e),
                (c,e) => _this.ModifyCounter?.Execute(c,e),
                (c,e) => _this.ModifyDamage?.Execute(c,e),
                (c,e) => _this.ModifyPlayer?.Execute(c,e),
                (c,e) => _this.MoveCard?.Execute(c,e),
                (c,e) => _this.SetVariable?.Execute(c,e),
                (c,e) => _this.Win?.Execute(c,e),
            };

            var result = effectEventArgs;
            var done = false;
            foreach (var action in actions)
            {
                var task = action(ownerCard, result);
                if (task == null) continue;

                var (done2, result2) = await task.Value;

                done = done || done2;
                result = result2;
            }

            return (done, result);
        }
    }
}
