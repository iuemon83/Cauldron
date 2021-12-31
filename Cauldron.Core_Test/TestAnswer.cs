using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;

namespace Cauldron.Core_Test
{
    internal class TestAnswer
    {
        public PlayerId[] ExpectedPlayerIdList = null;
        public CardId[] ExpectedCardIdList = null;
        public CardDefId[] ExpectedCardDefIdList = null;

        public PlayerId[] ChoicePlayerIdList = Array.Empty<PlayerId>();
        public CardId[] ChoiceCardIdList = Array.Empty<CardId>();
        public CardDefId[] ChoiceCardDefIdList = Array.Empty<CardDefId>();
    }
}
