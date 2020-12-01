namespace Cauldron.Server.Models.Effect
{
    public class EffectAction : IEffectAction
    {
        public EffectActionDamage Damage { get; set; }

        public EffectActionAddCard AddCard { get; set; }

        public EffectActionModifyCard ModifyCard { get; set; }

        public EffectActionDestroyCard DestroyCard { get; set; }

        public EffectActionModifyDamage ModifyDamage { get; set; }

        public bool Execute(Card ownerCard, EffectEventArgs effectEventArgs)
        {
            //TODO この順番もけっこう重要
            return
                (this.Damage?.Execute(ownerCard, effectEventArgs) ?? false)
                || (this.AddCard?.Execute(ownerCard, effectEventArgs) ?? false)
                || (this.ModifyCard?.Execute(ownerCard, effectEventArgs) ?? false)
                || (this.DestroyCard?.Execute(ownerCard, effectEventArgs) ?? false)
                || (this.ModifyDamage?.Execute(effectEventArgs) ?? false);
        }
    }
}
