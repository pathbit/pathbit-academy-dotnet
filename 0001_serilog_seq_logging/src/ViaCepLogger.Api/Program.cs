var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseDefaultSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar HttpClient para ViaCepService
builder.Services.AddHttpClient<ViaCepService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "ViaCepLogger/1.0");
});

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Adicionar Serilog request logging
app.UseDefaultSerilogRequestLogging();

// app.UseHttpsRedirection();  --- IGNORE ---

app.UseAuthentication();
app.UseAuthorization();

// Redirecionar rota raiz para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
   .ExcludeFromDescription();

app.MapControllers();

try
{
    Log.Information("Iniciando aplicação ViaCepLogger");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
