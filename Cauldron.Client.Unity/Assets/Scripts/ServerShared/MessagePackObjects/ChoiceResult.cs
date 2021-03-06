﻿using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ChoiceResult
    {
        public PlayerId[] PlayerIdList { get; } = Array.Empty<PlayerId>();
        public CardId[] CardIdList { get; } = Array.Empty<CardId>();
        public CardDefId[] CardDefIdList { get; } = Array.Empty<CardDefId>();

        public ChoiceResult(PlayerId[] PlayerIdList, CardId[] CardIdList, CardDefId[] CardDefIdList)
        {
            this.PlayerIdList = PlayerIdList;
            this.CardIdList = CardIdList;
            this.CardDefIdList = CardDefIdList;
        }
    }
}
