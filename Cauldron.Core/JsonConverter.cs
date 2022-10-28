using Cauldron.Shared.MessagePackObjects;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Cauldron.Core
{
    public class JsonConverter
    {
        private readonly static JsonSerializerOptions jsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            MaxDepth = 100,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new CardIdDictionaryJsonConverter<AttackTarget>()
            }
        };

        public static T? Deserialize<T>(string jsonText)
        {
            return JsonSerializer.Deserialize<T>(jsonText, jsonSerializerOptions);
        }

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }
    }

    public class CardIdDictionaryJsonConverter<TValue>
        : JsonConverter<IDictionary<CardId, AttackTarget>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            /* Only use this converter if 
			 * 1. It's a dictionary
			 * 2. The key is not a string
			 */
            if (typeToConvert != typeof(Dictionary<CardId, TValue>))
            {
                return false;
            }
            else if (typeToConvert.GenericTypeArguments.First() == typeof(string))
            {
                return false;
            }
            return true;
        }

        public override IDictionary<CardId, AttackTarget> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //Step 1 - Use built-in serializer to deserialize into a dictionary with string key
            var dictionaryWithStringKey = (Dictionary<string, AttackTarget>?)JsonSerializer.Deserialize(ref reader, typeof(Dictionary<string, AttackTarget>), options);

            //Step 2 - Convert the dictionary to one that uses the actual key type we want
            var dictionary = new Dictionary<CardId, AttackTarget>();

            if (dictionaryWithStringKey == null)
            {
                return dictionary;
            }

            foreach (var kvp in dictionaryWithStringKey)
            {
                dictionary.Add(CardId.Parse(kvp.Key), kvp.Value);
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<CardId, AttackTarget> value, JsonSerializerOptions options)
        {
            //Step 1 - Convert dictionary to a dictionary with string key
            var dictionary = new Dictionary<string, AttackTarget>(value.Count);

            foreach (var kvp in value)
            {
                dictionary.Add(kvp.Key.ToString(), kvp.Value);
            }
            //Step 2 - Use the built-in serializer, because it can handle dictionaries with string keys
            JsonSerializer.Serialize(writer, dictionary, options);
        }
    }
}
