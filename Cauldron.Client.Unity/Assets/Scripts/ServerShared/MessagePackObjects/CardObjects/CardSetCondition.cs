﻿#nullable enable

using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardSetCondition
    {
        public enum TypeValue
        {
            [DisplayText("このセット")]
            This,
            [DisplayText("その他のセット")]
            Other
        }

        public TypeValue Type { get; } = TypeValue.This;

        public TextCompare? ValueCondition { get; }

        public CardSetCondition(TypeValue Type, TextCompare? ValueCondition = null)
        {
            this.Type = Type;
            this.ValueCondition = ValueCondition;
        }
    }
}
