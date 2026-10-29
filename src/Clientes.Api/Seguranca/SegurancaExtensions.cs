using System.Threading.RateLimiting;
using Clientes.Infrastructure.Autenticacao;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Clientes.Api.Seguranca;

internal static class SegurancaExtensions
{
    public const string PoliticaLogin = "login";
    public const int TentativasDeLoginPorMinuto = 5;

    public static IServiceCollection AddSeguranca(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AutenticacaoOptions>>((jwt, autenticacao) =>
            {
                var configurado = autenticacao.Value;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configurado.Emissor,
                    ValidAudience = configurado.Audiencia,
                    IssuerSigningKey = configurado.CriarChave(),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = AutenticacaoOptions.ClaimPapel
                };
            });

        services.AddAuthorization();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PoliticaLogin, http => RateLimitPartition.GetFixedWindowLimiter(
                IpDoCliente(http),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = TentativasDeLoginPorMinuto,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });

        return services;
    }

    private static string IpDoCliente(HttpContext http)
    {
        var encaminhado = http.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        return encaminhado.Length > 0 ? encaminhado : http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
    }
}
