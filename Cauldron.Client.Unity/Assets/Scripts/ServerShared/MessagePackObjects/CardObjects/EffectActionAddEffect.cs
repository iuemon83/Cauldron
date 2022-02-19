#nullable enable

using MessagePack;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 効果を付与する
    /// </summary>
    [MessagePackObject(true)]
    public class EffectActionAddEffect
    {
        public Choice CardsChoice { get; }
        public IEnumerable<CardEffect> EffectToAdd { get; }
        public string? Name { get; } = null;

        public EffectActionAddEffect(Choice CardsChoice, IEnumerable<CardEffect> EffectToAdd, string? Name = null)
        {
            this.CardsChoice = CardsChoice;
            this.EffectToAdd = EffectToAdd;
            this.Name = Name;
        }
    }
}