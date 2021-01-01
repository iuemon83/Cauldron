using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public record EffectAction(
        EffectActionDamage Damage = null,
        EffectActionAddCard AddCard = null,
        EffectActionModifyCard ModifyCard = null,
        EffectActionDestroyCard DestroyCard = null,
        EffectActionModifyDamage ModifyDamage = null,
        EffectActionModifyPlayer ModifyPlayer = null,
        EffectActionDrawCard DrawCard = null,
        EffectActionMoveCard MoveCard = null,
        EffectActionAddEffect AddEffect = null
        ) : IEffectAction
    {
        /// <summary>
        /// テストようにvirtual にしてる
        /// </summary>
        /// <param name="ownerCard"></param>
        /// <param name="effectEventArgs"></param>
        /// <returns></returns>
        public virtual (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs effectEventArgs)
        {
            //TODO この順番もけっこう重要
            var actions = new IEffectAction[]
            {
                this.Damage,
                this.AddCard,
                this.ModifyCard,
                this.DestroyCard,
                this.ModifyDamage,
                this.ModifyPlayer,
                this.DrawCard,
                this.MoveCard,
                this.AddEffect,
            };

            var result = effectEventArgs;
            var done = false;
            foreach (var action in actions.Where(a => a != null))
            {
                var (done2, result2) = action.Execute(ownerCard, result);

                done = done || done2;
                result = result2;
            }

            return (done, result);
        }
    }
}
