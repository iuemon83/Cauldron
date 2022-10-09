#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectAction
    {
        /// <summary>
        /// EffectConditionとの違いは、こっちはActionの処理時に初めて評価される
        /// EffectConditionはイベント発生時に評価される
        /// 
        /// 「～して、そのカードが～なら」のようなときに利用する
        /// </summary>
        public EffectIf? If { get; } = null;

        public EffectActionDamage? Damage { get; } = null;
        public EffectActionHeal? Heal { get; } = null;
        public EffectActionAddCard? AddCard { get; } = null;
        public EffectActionExcludeCard? ExcludeCard { get; } = null;
        public EffectActionModifyCard? ModifyCard { get; } = null;
        public EffectActionDestroyCard? DestroyCard { get; } = null;
        public EffectActionModifyDamage? ModifyDamage { get; } = null;
        public EffectActionModifyPlayer? ModifyPlayer { get; } = null;
        public EffectActionDrawCard? DrawCard { get; } = null;
        public EffectActionMoveCard? MoveCard { get; } = null;
        public EffectActionModifyNumFields? ModifyNumFields { get; } = null;
        public EffectActionAddEffect? AddEffect { get; } = null;
        public EffectActionSetVariable? SetVariable { get; } = null;
        public EffectActionModifyCounter? ModifyCounter { get; } = null;
        public EffectActionWin? Win { get; } = null;
        public EffectActionReserveEffect? ReserveEffect { get; } = null;

        public EffectAction(
            EffectIf? If = null,
            EffectActionDamage? Damage = null,
            EffectActionHeal? Heal = null,
            EffectActionAddCard? AddCard = null,
            EffectActionExcludeCard? ExcludeCard = null,
            EffectActionModifyCard? ModifyCard = null,
            EffectActionDestroyCard? DestroyCard = null,
            EffectActionModifyDamage? ModifyDamage = null,
            EffectActionModifyPlayer? ModifyPlayer = null,
            EffectActionDrawCard? DrawCard = null,
            EffectActionMoveCard? MoveCard = null,
            EffectActionModifyNumFields? ModifyNumFields = null,
            EffectActionAddEffect? AddEffect = null,
            EffectActionSetVariable? SetVariable = null,
            EffectActionModifyCounter? ModifyCounter = null,
            EffectActionWin? Win = null,
            EffectActionReserveEffect? ReserveEffect = null
            )
        {
            this.If = If;
            this.Damage = Damage;
            this.Heal = Heal;
            this.AddCard = AddCard;
            this.ExcludeCard = ExcludeCard;
            this.ModifyCard = ModifyCard;
            this.DestroyCard = DestroyCard;
            this.ModifyDamage = ModifyDamage;
            this.ModifyPlayer = ModifyPlayer;
            this.DrawCard = DrawCard;
            this.MoveCard = MoveCard;
            this.ModifyNumFields = ModifyNumFields;
            this.AddEffect = AddEffect;
            this.SetVariable = SetVariable;
            this.ModifyCounter = ModifyCounter;
            this.Win = Win;
            this.ReserveEffect = ReserveEffect;
        }
    }
}
