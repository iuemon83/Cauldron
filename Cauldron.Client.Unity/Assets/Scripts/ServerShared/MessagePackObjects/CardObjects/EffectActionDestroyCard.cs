﻿using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果で、カードを破壊する処理
    /// </summary>
    [MessagePackObject(true)]
    public class EffectActionDestroyCard
    {
        public Choice Choice { get; set; }
        public string Name { get; set; } = null;

        public EffectActionDestroyCard(Choice choice, string name = null)
        {
            this.Choice = choice;
            this.Name = name;
        }
    }
}
