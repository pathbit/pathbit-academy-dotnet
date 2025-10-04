namespace ViaCepLogger.Api.Services;

public class ViaCepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ViaCepResponse?> GetAddressByCepAsync(string cep)
    {
        try
        {
            _logger.LogInformation("Iniciando consulta de CEP {Cep}", cep);

            var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("CEP {Cep} não encontrado (404)", cep);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao consultar CEP {Cep}. Status: {StatusCode}",
                    cep,
                    (int)response.StatusCode
                );
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var address = JsonSerializer.Deserialize<ViaCepResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (address?.Erro == true)
            {
                _logger.LogWarning("CEP {Cep} retornou erro na resposta da API", cep);
                return null;
            }

            _logger.LogInformation(
                "CEP {Cep} consultado com sucesso. Cidade: {Cidade}, UF: {UF}",
                cep,
                address?.Localidade,
                address?.Uf
            );

            return address;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Erro de rede ao consultar CEP {Cep}. Mensagem: {ErrorMessage}",
                cep,
                ex.Message
            );
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Timeout ao consultar CEP {Cep}",
                cep
            );
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao deserializar resposta do CEP {Cep}",
                cep
            );
            return null;
        }
    }
}
