using Asp.Versioning;
using Clientes.Api.Seguranca;
using Clientes.Application.Autenticacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clientes.Api.Controllers;

public sealed record LoginRequest(string Usuario, string Senha);

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public sealed class AuthController(AutenticarHandler autenticar) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(SegurancaExtensions.PoliticaLogin)]
    [EndpointSummary("Autentica o moderador e devolve um token JWT.")]
    [EndpointDescription("Envie o token no header Authorization: Bearer {token} para acessar a listagem de clientes. Limite de 5 tentativas por minuto por IP.")]
    [ProducesResponseType<TokenAcesso>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult Login(LoginRequest request) =>
        Ok(autenticar.Handle(new AutenticarCommand(request.Usuario, request.Senha)));
}
