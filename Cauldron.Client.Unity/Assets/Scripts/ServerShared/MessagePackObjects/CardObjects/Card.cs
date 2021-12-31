﻿using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class Card
    {
        public CardId Id { get; set; }

        public PlayerId OwnerId { get; set; }

        public CardDefId CardDefId { get; set; }

        public int BaseCost { get; set; }

        public int CostBuff { get; set; }

        public string CardSetName { get; set; }

        public string Name { get; set; }

        public string FullName => $"{this.CardSetName}.{this.Name}";

        public string FlavorText { get; set; }

        public List<string> Annotations { get; set; }

        public bool IsToken { get; set; }

        public CardType Type { get; set; }

        public int BasePower { get; set; }

        public int BaseToughness { get; set; }

        public int PowerBuff { get; set; }

        public int ToughnessBuff { get; set; }

        public int Cost => Math.Max(0, this.BaseCost + this.CostBuff);

        public int Power => Math.Max(0, this.BasePower + this.PowerBuff);

        public int Toughness => Math.Max(0, this.BaseToughness + this.ToughnessBuff);

        public Zone Zone { get; set; }

        public List<CreatureAbility> Abilities { get; set; }

        public string EffectText { get; set; }

        public List<CardEffect> Effects { get; set; }

        public Dictionary<string, int> CountersByName { get; set; }
            = new Dictionary<string, int>();

        /// <summary>
        /// 攻撃可能となるまでのターン数
        /// </summary>
        public int NumTurnsToCanAttack { get; set; }

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

        public bool HasAbility(CreatureAbility ability) => this.Abilities.Contains(ability);

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

            switch (ability)
            {
                case CreatureAbility.Cover:
                    return this.HasAbility(ability) && !this.HasAbility(CreatureAbility.Stealth);

                default:
                    return this.HasAbility(ability);
            }
        }

        /// <summary>
        /// 攻撃可能な状態か
        /// </summary>
        /// <param name="attackCard"></param>
        /// <returns></returns>
        public bool CanAttack =>
            // クリーチャーでなければ攻撃できない
            this.Type == CardType.Creature
            // 場にいなければ攻撃できない
            && this.Zone.ZoneName == ZoneName.Field
            // 召喚酔いでない
            && this.NumTurnsToCanAttack <= this.NumTurnsInField
            // 攻撃不能状態でない
            && !this.EnableAbility(CreatureAbility.CantAttack)
            // 1ターン中に攻撃可能な回数を超えていない
            && this.NumAttacksLimitInTurn > this.NumAttacksInTurn
            ;

        public Card() { }

        public Card(CardDef cardDef)
        {
            this.Id = CardId.NewId();
            this.CardDefId = cardDef.Id;
            this.BaseCost = cardDef.Cost;
            this.IsToken = cardDef.IsToken;
            this.Type = cardDef.Type;
            this.CardSetName = cardDef.CardSetName;
            this.Name = cardDef.Name;
            this.FlavorText = cardDef.FlavorText;
            this.Annotations = cardDef.Annotations.ToList();

            this.BasePower = cardDef.Power;
            this.BaseToughness = cardDef.Toughness;

            this.Abilities = cardDef.Abilities.ToList();
            this.EffectText = cardDef.EffectText;
            this.Effects = cardDef.Effects.ToList();
            this.NumTurnsToCanAttack = cardDef.NumTurnsToCanAttack.Value;
            this.NumAttacksLimitInTurn = cardDef.NumAttacksLimitInTurn.Value;
        }

        public override string ToString()
        {
            switch (this.Type)
            {
                case CardType.Artifact:
                    return $"{this.Name}[{this.Cost}]";

                case CardType.Sorcery:
                    return $"{this.Name}[{this.Cost}]";

                default:
                    return $"{this.Name}[{this.Cost},{this.Power},{this.Toughness}]";
            }
        }

        public void ModifyCounter(string name, int addValue)
        {
            if (!this.CountersByName.TryGetValue(name, out var num))
            {
                this.CountersByName.Add(name, 0);
            }

            this.CountersByName[name] = num + addValue;
        }

        public int GetCounter(string name)
        {
            return this.CountersByName.TryGetValue(name, out var num)
                ? num
                : 0;
        }
    }
}
