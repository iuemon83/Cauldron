using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Cauldron.Core.Entities
{
    public class JsonConverter
    {
        public static T Deserialize<T>(string jsonText)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                MaxDepth = 100,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return JsonSerializer.Deserialize<T>(jsonText, options);
        }

        public static string Serialize<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                MaxDepth = 100,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return JsonSerializer.Serialize(obj, options);
        }
    }
}
