using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectAction
    {
        public EffectActionDamage Damage { get; } = null;
        public EffectActionAddCard AddCard { get; } = null;
        public EffectActionModifyCard ModifyCard { get; } = null;
        public EffectActionDestroyCard DestroyCard { get; } = null;
        public EffectActionModifyDamage ModifyDamage { get; } = null;
        public EffectActionModifyPlayer ModifyPlayer { get; } = null;
        public EffectActionDrawCard DrawCard { get; } = null;
        public EffectActionMoveCard MoveCard { get; } = null;
        public EffectActionAddEffect AddEffect { get; } = null;
        public EffectActionSetVariable SetVariable { get; } = null;
        public EffectActionWin Win { get; } = null;

        public EffectAction(
            EffectActionDamage Damage = null,
            EffectActionAddCard AddCard = null,
            EffectActionModifyCard ModifyCard = null,
            EffectActionDestroyCard DestroyCard = null,
            EffectActionModifyDamage ModifyDamage = null,
            EffectActionModifyPlayer ModifyPlayer = null,
            EffectActionDrawCard DrawCard = null,
            EffectActionMoveCard MoveCard = null,
            EffectActionAddEffect AddEffect = null,
            EffectActionSetVariable SetVariable = null,
            EffectActionWin Win = null
            )
        {
            this.Damage = Damage;
            this.AddCard = AddCard;
            this.ModifyCard = ModifyCard;
            this.DestroyCard = DestroyCard;
            this.ModifyDamage = ModifyDamage;
            this.ModifyPlayer = ModifyPlayer;
            this.DrawCard = DrawCard;
            this.MoveCard = MoveCard;
            this.AddEffect = AddEffect;
            this.AddEffect = AddEffect;
            this.SetVariable = SetVariable;
            this.Win = Win;
        }
    }
}
