namespace ViaCepLogger.Api.Infrastructure.Converters;

/// <summary>
/// Converter customizado para lidar com a API do ViaCEP que retorna
/// "erro": "true" como string em vez de boolean
/// </summary>
public class StringToBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            return bool.TryParse(stringValue, out var result) && result;
        }

        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        return false;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
