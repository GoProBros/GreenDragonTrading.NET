using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.Common.Utils
{
    /// <summary>
    /// Handles JSON fields that can arrive as either a JSON string or a JSON number,
    /// deserializing both into a C# <see langword="string"/>.
    /// 
    /// Required for MoMo legacy IPN payloads where fields such as <c>transId</c> and
    /// <c>amount</c> are sent as numbers by the MoMo gateway but are treated as strings
    /// in our domain model. Without this converter, <c>System.Text.Json</c> throws a
    /// deserialization exception, causing ASP.NET Core to return 400 before the controller
    /// action is entered — silently breaking IPN processing.
    /// </summary>
    public class JsonNumberOrStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var longVal)
                    ? longVal.ToString()
                    : reader.GetDouble().ToString(),
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Cannot convert token type {reader.TokenType} to string.")
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }
}
