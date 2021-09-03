using Cauldron.Core.Entities;
using System;

namespace Cauldron.Server
{
    public class CardPoolInitializer
    {
        public static void Init(string cardsetDirectoryPath)
        {
            var cardpool = CardPool.ReadFromDirectory(cardsetDirectoryPath);
            if (!cardpool.IsValid())
            {
                throw new InvalidOperationException("カードプールが不正です。");
            }
        }

        public static CardPool CardPoolSingleton { get; private set; }
    }
}
