using System.Security.Claims;
using Clientes.Application.Abstractions;
using Clientes.Application.Autenticacao;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Clientes.Infrastructure.Autenticacao;

internal sealed class GeradorTokenJwt(IOptions<AutenticacaoOptions> opcoes, TimeProvider relogio) : IGeradorToken
{
    private readonly JsonWebTokenHandler _handler = new();

    public TokenAcesso Gerar(string usuario)
    {
        var configurado = opcoes.Value;
        var agora = relogio.GetUtcNow();
        var expiraEm = agora.AddMinutes(configurado.ExpiracaoMinutos);

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = configurado.Emissor,
            Audience = configurado.Audiencia,
            IssuedAt = agora.UtcDateTime,
            NotBefore = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Trim().ToLowerInvariant()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(AutenticacaoOptions.ClaimPapel, Papeis.Moderador)
            ]),
            SigningCredentials = new SigningCredentials(configurado.CriarChave(), SecurityAlgorithms.HmacSha256)
        });

        return new TokenAcesso(token, "Bearer", expiraEm);
    }
}
