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
            if (bool.TryParse(stringValue, out var result))
                return result;
            throw new JsonException("O campo erro deve conter um booleano ou a string true/false.");
        }

        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        throw new JsonException("Tipo inválido para o campo erro retornado pelo ViaCEP.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
