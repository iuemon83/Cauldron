#nullable enable

using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardDef : ICardDef
    {
        public static CardDef Empty => new CardDef();

        public CardDefId Id { get; private set; }

        public int Cost { get; set; }

        [JsonIgnore]
        public string CardSetName { get; set; } = "";

        public string Name { get; set; } = "";

        [JsonIgnore]
        public string FullName => $"{this.CardSetName}.{this.Name}";

        public string FlavorText { get; set; } = "";

        public IReadOnlyList<string> Annotations { get; set; } = Array.Empty<string>();

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int Power { get; set; } = 0;

        public int Toughness { get; set; } = 0;

        public IReadOnlyList<CreatureAbility> Abilities { get; set; } = Array.Empty<CreatureAbility>();

        public IReadOnlyList<CardEffect> Effects { get; set; } = Array.Empty<CardEffect>();

        /// <summary>
        /// クリーチャーへ攻撃可能となるまでのターン数
        /// </summary>
        public int? NumTurnsToCanAttackToCreature { get; set; }

        /// <summary>
        /// プレイヤーへ攻撃可能となるまでのターン数
        /// </summary>
        public int? NumTurnsToCanAttackToPlayer { get; set; }

        /// <summary>
        /// 1ターン中に攻撃可能な回数
        /// </summary>
        public int? NumAttacksLimitInTurn { get; set; }

        /// <summary>
        /// デッキに入れることのできる枚数
        /// </summary>
        public int? LimitNumCardsInDeck { get; set; }

        public string EffectDescription => string.Join(Environment.NewLine, this.Effects.Select(x => x.Description));

        [JsonConstructor]
        public CardDef(
            CardDefId Id = default,
            int Cost = default,
            string? CardSetName = default,
            string? Name = default,
            string? FlavorText = default,
            IReadOnlyList<string>? Annotations = default,
            bool IsToken = default,
            CardType Type = default,
            int Power = default,
            int Toughness = default,
            IReadOnlyList<CreatureAbility>? Abilities = default,
            IReadOnlyList<CardEffect>? Effects = default,
            int? NumTurnsToCanAttackToCreature = default,
            int? NumTurnsToCanAttackToPlayer = default,
            int? NumAttacksLimitInTurn = default,
            int? LimitNumCardsInDeck = default
            )
        {
            this.Id = Id;
            this.Cost = Cost;
            this.CardSetName = CardSetName ?? "";
            this.Name = Name ?? "";
            this.FlavorText = FlavorText ?? "";
            this.Annotations = Annotations ?? Array.Empty<string>();
            this.IsToken = IsToken;
            this.Type = Type;
            this.Power = Power;
            this.Toughness = Toughness;
            this.Abilities = Abilities ?? Array.Empty<CreatureAbility>();
            this.Effects = Effects ?? Array.Empty<CardEffect>();
            this.NumTurnsToCanAttackToCreature = NumTurnsToCanAttackToCreature;
            this.NumTurnsToCanAttackToPlayer = NumTurnsToCanAttackToPlayer;
            this.NumAttacksLimitInTurn = NumAttacksLimitInTurn;
            this.LimitNumCardsInDeck = LimitNumCardsInDeck;
        }

        /// <summary>
        /// cardset のjson からロードするときはIDがemptyで入っちゃうので振りなおす用
        /// それ以外では利用しない
        /// </summary>
        public void DangerousSetNewId()
        {
            this.Id = CardDefId.NewId();
        }
    }
}
