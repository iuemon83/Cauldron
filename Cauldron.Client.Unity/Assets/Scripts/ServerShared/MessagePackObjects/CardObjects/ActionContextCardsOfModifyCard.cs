﻿using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード修整アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfModifyCard
    {
        public enum TypeValue
        {
            [DisplayText("修整されたカード")]
            Modified
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfModifyCard(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
