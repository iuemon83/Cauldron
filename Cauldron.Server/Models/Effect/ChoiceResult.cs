using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public class ChoiceResult
    {
        public IReadOnlyList<Guid> PlayerIdList { get; set; } = new Guid[0];
        public IReadOnlyList<Card> CardList { get; set; } = new Card[0];
        public IReadOnlyList<CardDef> CardDefList { get; set; } = new CardDef[0];
    }
}
