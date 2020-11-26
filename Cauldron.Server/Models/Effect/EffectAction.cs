using System;

namespace Cauldron.Server.Models.Effect
{
    public class EffectAction : IEffectAction
    {
        public EffectActionDamage Damage { get; set; }

        public EffectActionAddCard AddCard { get; set; }

        public EffectActionModifyCard ModifyCard { get; set; }

        public EffectActionDestroyCard DestroyCard { get; set; }

        public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource)
        {
            //TODO この順番もけっこう重要
            this.Damage?.Execute(gameMaster, ownerCard, eventSource);
            this.AddCard?.Execute(gameMaster, ownerCard, eventSource);
            this.ModifyCard?.Execute(gameMaster, ownerCard, eventSource);
            this.DestroyCard?.Execute(gameMaster, ownerCard, eventSource);
        }

        public Action<EffectEventArgs> Execute(Card ownerCard)
        {
            return args =>
            {
                //TODO この順番もけっこう重要
                this.Damage?.Execute(ownerCard, args);
                this.AddCard?.Execute(ownerCard, args);
                this.ModifyCard?.Execute(ownerCard, args);
                this.DestroyCard?.Execute(ownerCard, args);
            };
        }
    }
}
