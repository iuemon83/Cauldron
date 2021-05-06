﻿using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    [Serializable]
    class Deck : IDeck
    {
        public string Id;
        public string Name;
        public string[] PackNames;
        public string[] CardDefNames;

        string IDeck.Id => this.Id;

        string IDeck.Name => this.Name;

        IReadOnlyList<string> IDeck.PackNames => this.PackNames;

        IReadOnlyList<string> IDeck.CardDefNames => this.CardDefNames;
    }
}
