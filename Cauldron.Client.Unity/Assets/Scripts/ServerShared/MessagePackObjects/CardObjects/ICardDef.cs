using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public interface ICardDef
    {
        public CardDefId Id { get; }

        public int Cost { get; }

        public string CardSetName { get; }

        public string Name { get; }

        public string FullName { get; }

        public string FlavorText { get; }

        public IReadOnlyList<string> Annotations { get; }

        public bool IsToken { get; }

        public CardType Type { get; }

        public int Power { get; }

        public int Toughness { get; }

        public IReadOnlyList<CreatureAbility> Abilities { get; }

        public IReadOnlyList<CardEffect> Effects { get; }

        /// <summary>
        /// クリーチャーへ攻撃可能となるまでのターン数
        /// </summary>
        public int? NumTurnsToCanAttackToCreature { get; }

        /// <summary>
        /// プレイヤーへ攻撃可能となるまでのターン数
        /// </summary>
        public int? NumTurnsToCanAttackToPlayer { get; }

        /// <summary>
        /// 1ターン中に攻撃可能な回数
        /// </summary>
        public int? NumAttacksLimitInTurn { get; }

        /// <summary>
        /// デッキに入れることのできる枚数
        /// </summary>
        public int? LimitNumCardsInDeck { get; }

        public string EffectDescription { get; }
    }
}
