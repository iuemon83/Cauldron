using Cauldron.Core;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.IO;

namespace Cauldron.Server
{
    public class RuleBookInitializer
    {
        private static RuleBook ReadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            return JsonConverter.Deserialize<RuleBook>(jsonString);
        }

        public static void Init(string jsonFilePath)
        {
            RuleBookSingleton = ReadFromFile(jsonFilePath);

            if (RuleBookSingleton == default)
            {
                throw new InvalidOperationException("invalid rule book");
            }
        }

        public static RuleBook RuleBookSingleton { get; private set; }
    }
}
