using Asp.Versioning;
using Clientes.Api.Middlewares;
using Clientes.Api.Seguranca;
using Clientes.Application;
using Clientes.Infrastructure;
using Clientes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

if (Environment.GetEnvironmentVariable("PORT") is { } porta)
    builder.WebHost.UseUrls($"http://0.0.0.0:{porta}");

builder.Host.UseSerilog((contexto, log) =>
{
    log.ReadFrom.Configuration(contexto.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Servico", "clientes-api");

    if (contexto.HostingEnvironment.IsDevelopment())
        log.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}");
    else
        log.WriteTo.Console(new RenderedCompactJsonFormatter());
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSeguranca();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clientes API",
        Version = "v1",
        Description = "Cadastro de clientes. Cada cadastro publica o evento ClienteCadastrado. A listagem exige token de moderador."
    });
    options.AddSecurityDefinition(AutorizacaoSwaggerFilter.Esquema, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtido em POST /api/v1/auth/login."
    });
    options.OperationFilter<AutorizacaoSwaggerFilter>();
});

var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(origens)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Location", "X-Correlation-Id")));

var app = builder.Build();

if (app.Configuration.GetValue("Database:AplicarMigrations", true))
{
    await using var escopo = app.Services.CreateAsyncScope();
    await escopo.ServiceProvider.GetRequiredService<ClientesDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clientes API v1"));

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
