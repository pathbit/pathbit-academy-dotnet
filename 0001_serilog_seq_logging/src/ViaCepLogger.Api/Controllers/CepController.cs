namespace ViaCepLogger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CepController : ControllerBase
{
    private readonly ViaCepService _viaCepService;
    private readonly ILogger<CepController> _logger;

    public CepController(ViaCepService viaCepService, ILogger<CepController> logger)
    {
        _viaCepService = viaCepService;
        _logger = logger;
    }

    [HttpGet("{cep}")]
    public async Task<IActionResult> GetAddress(string cep)
    {
        _logger.LogInformation("Requisição recebida para consulta de CEP {Cep}", cep);

        // Remove caracteres não numéricos
        var cleanCep = Regex.Replace(cep, @"[^\d]", "");

        // Valida formato do CEP
        if (cleanCep.Length != 8 || !Regex.IsMatch(cleanCep, @"^\d{8}$"))
        {
            _logger.LogWarning(
                "CEP {Cep} possui formato inválido. CEP limpo: {CleanCep}",
                cep,
                cleanCep
            );
            return BadRequest(new
            {
                error = "Formato de CEP inválido",
                message = "O CEP deve conter exatamente 8 dígitos numéricos"
            });
        }

        var address = await _viaCepService.GetAddressByCepAsync(cleanCep);

        if (address == null)
        {
            _logger.LogWarning("CEP {Cep} não encontrado ou erro na consulta", cleanCep);
            return NotFound(new
            {
                error = "CEP não encontrado",
                message = $"Não foi possível encontrar informações para o CEP {cleanCep}"
            });
        }

        _logger.LogInformation(
            "Requisição concluída com sucesso para CEP {Cep}",
            cleanCep
        );

        return Ok(address);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check executado");
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
