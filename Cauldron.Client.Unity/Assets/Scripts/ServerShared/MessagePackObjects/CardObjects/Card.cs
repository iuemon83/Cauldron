#nullable enable

using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class Card
    {
        public static Card Empty => new Card(CardDef.Empty);

        public CardId Id { get; set; }

        public PlayerId OwnerId { get; set; }

        public CardDefId CardDefId { get; set; }

        public int BaseCost { get; set; }

        public string CardSetName { get; set; } = "";

        public string Name { get; set; } = "";

        public string FlavorText { get; set; } = "";

        public List<string> Annotations { get; set; } = new List<string>();

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int BasePower { get; set; }

        public int BaseToughness { get; set; }

        public Zone Zone { get; set; } = Zone.Empty;

        public List<CreatureAbility> Abilities { get; set; } = new List<CreatureAbility>();

        public List<CardEffect> Effects { get; set; } = new List<CardEffect>();

        public Dictionary<string, int> CountersByName { get; set; }
            = new Dictionary<string, int>();

        public int CostBuff { get; set; }

        public int PowerBuff { get; set; }

        public int ToughnessBuff { get; set; }

        /// <summary>
        /// クリーチャーへ攻撃可能となるまでのターン数
        /// </summary>
        public int NumTurnsToCanAttackToCreature { get; set; }

        /// <summary>
        /// プレイヤーへ攻撃可能となるまでのターン数
        /// </summary>
        public int NumTurnsToCanAttackToPlayer { get; set; }

        /// <summary>
        /// 1ターン中に攻撃可能な回数
        /// </summary>
        public int NumAttacksLimitInTurn { get; set; }

        /// <summary>
        /// 1ターン中に攻撃した回数
        /// </summary>
        public int NumAttacksInTurn { get; set; }

        /// <summary>
        /// フィールドに出てからのターン数
        /// </summary>
        public int NumTurnsInField { get; set; }

        private readonly CardDef? cardDef;

        public string FullName => $"{this.CardSetName}.{this.Name}";

        public int Cost => Math.Max(0, this.BaseCost + this.CostBuff);

        public int Power => Math.Max(0, this.BasePower + this.PowerBuff);

        public int Toughness => Math.Max(0, this.BaseToughness + this.ToughnessBuff);

        public bool HasAbility(CreatureAbility ability) => this.Abilities.Contains(ability);

        public string EffectDescription => string.Join(Environment.NewLine, this.Effects.Select(x => x.Description));

        /// <summary>
        /// 指定したアビリティが有効になっている場合はtrue、そうでなければfalse
        /// </summary>
        /// <param name="card"></param>
        /// <param name="ability"></param>
        /// <returns></returns>
        public bool EnableAbility(CreatureAbility ability)
        {
            if (this.Zone.ZoneName != ZoneName.Field)
            {
                return false;
            }

            if (this.HasAbility(CreatureAbility.Sealed))
            {
                return ability == CreatureAbility.Sealed;
            }

            return ability switch
            {
                CreatureAbility.Cover => this.HasAbility(ability) && !this.HasAbility(CreatureAbility.Stealth),
                _ => this.HasAbility(ability),
            };
        }

        public Card(
            CardId Id,
            CardDefId CardDefId,
            int BaseCost,
            bool IsToken,
            CardType Type,
            string CardSetName,
            string Name,
            string FlavorText,
            IReadOnlyList<string> Annotations,
            int BasePower,
            int BaseToughness,
            IReadOnlyList<CreatureAbility> Abilities,
            IReadOnlyList<CardEffect> Effects,
            int NumTurnsToCanAttackToCreature,
            int NumTurnsToCanAttackToPlayer,
            int NumAttacksLimitInTurn
            )
        {
            this.Init(
                Id,
                CardDefId,
                BaseCost,
                IsToken,
                Type,
                CardSetName,
                Name,
                FlavorText,
                Annotations.ToList(),
                BasePower,
                BaseToughness,
                Abilities.ToList(),
                Effects.ToList(),
                NumTurnsToCanAttackToCreature,
                NumTurnsToCanAttackToPlayer,
                NumAttacksLimitInTurn
                );
        }

        public Card(CardDef cardDef)
        {
            this.Id = CardId.NewId();
            this.cardDef = cardDef;

            this.Init(cardDef);
        }

        private void Init(CardDef cardDef)
        {
            this.Init(
                this.Id,
                cardDef.Id,
                cardDef.Cost,
                cardDef.IsToken,
                cardDef.Type,
                cardDef.CardSetName,
                cardDef.Name,
                cardDef.FlavorText,
                cardDef.Annotations.ToList(),
                cardDef.Power,
                cardDef.Toughness,
                cardDef.Abilities.ToList(),
                cardDef.Effects.Select(x => x.CloneWithNewId()).ToList(),
                cardDef.NumTurnsToCanAttackToCreature ?? default,
                cardDef.NumTurnsToCanAttackToPlayer ?? default,
                cardDef.NumAttacksLimitInTurn ?? default
                );
        }

        private void Init(
            CardId Id,
            CardDefId CardDefId,
            int BaseCost,
            bool IsToken,
            CardType Type,
            string CardSetName,
            string Name,
            string FlavorText,
            IReadOnlyList<string> Annotations,
            int BasePower,
            int BaseToughness,
            IReadOnlyList<CreatureAbility> Abilities,
            IReadOnlyList<CardEffect> Effects,
            int NumTurnsToCanAttackToCreature,
            int NumTurnsToCanAttackToPlayer,
            int NumAttacksLimitInTurn
            )
        {
            this.Id = Id;
            this.CardDefId = CardDefId;
            this.BaseCost = BaseCost;
            this.IsToken = IsToken;
            this.Type = Type;
            this.CardSetName = CardSetName;
            this.Name = Name;
            this.FlavorText = FlavorText;
            this.Annotations = Annotations.ToList();
            this.BasePower = BasePower;
            this.BaseToughness = BaseToughness;
            this.Abilities = Abilities.ToList();
            this.Effects = Effects.ToList();
            this.NumTurnsToCanAttackToCreature = NumTurnsToCanAttackToCreature;
            this.NumTurnsToCanAttackToPlayer = NumTurnsToCanAttackToPlayer;
            this.NumAttacksLimitInTurn = NumAttacksLimitInTurn;

            this.Zone = Zone.Empty;
            this.CountersByName.Clear();
        }

        public void Reset()
        {
            if (this.cardDef != null)
            {
                this.Init(this.cardDef);
            }

            this.NumAttacksInTurn = 0;
            this.NumTurnsInField = 0;
            this.CostBuff = 0;
            this.PowerBuff = 0;
            this.ToughnessBuff = 0;
        }

        public override string ToString()
        {
            return this.Type switch
            {
                CardType.Artifact => $"{this.Name}[{this.Cost}]",
                CardType.Sorcery => $"{this.Name}[{this.Cost}]",
                _ => $"{this.Name}[{this.Cost},{this.Power},{this.Toughness}]",
            };
        }

        public void ModifyCounter(string name, int addValue)
        {
            if (!this.CountersByName.TryGetValue(name, out var num))
            {
                this.CountersByName.Add(name, 0);
            }

            var newNum = num + addValue;

            if (newNum > 0)
            {
                this.CountersByName[name] = newNum;
            }
            else
            {
                this.CountersByName.Remove(name);
            }
        }

        public int GetCounter(string name)
        {
            return this.CountersByName.TryGetValue(name, out var num)
                ? num
                : 0;
        }

        public Card AsHidden()
        {
            return new Card(
                default,
                default,
                default,
                default,
                default,
                "",
                "非公開",
                "",
                Array.Empty<string>(),
                default,
                default,
                Array.Empty<CreatureAbility>(),
                Array.Empty<CardEffect>(),
                default,
                default,
                default
                )
            {
                Zone = this.Zone
            };
        }
    }
}
