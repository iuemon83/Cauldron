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
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectAction effectAction, Card ownerCard, EffectEventArgs effectEventArgs)
        {
            //TODO この順番もけっこう重要
            var actions = new Func<Card, EffectEventArgs, ValueTask<(bool, EffectEventArgs)>?>[]
            {
                (c,e) => effectAction.Damage?.Execute(c,e),
                (c,e) => effectAction.AddCard?.Execute(c,e),
                (c,e) => effectAction.ExcludeCard?.Execute(c,e),
                (c,e) => effectAction.ModifyCard?.Execute(c,e),
                (c,e) => effectAction.DestroyCard?.Execute(c,e),
                (c,e) => effectAction.ModifyDamage?.Execute(c,e),
                (c,e) => effectAction.ModifyPlayer?.Execute(c,e),
                (c,e) => effectAction.DrawCard?.Execute(c,e),
                (c,e) => effectAction.MoveCard?.Execute(c,e),
                (c,e) => effectAction.AddEffect?.Execute(c,e),
                (c,e) => effectAction.SetVariable?.Execute(c,e),
                (c,e) => effectAction.Win?.Execute(c,e),
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
