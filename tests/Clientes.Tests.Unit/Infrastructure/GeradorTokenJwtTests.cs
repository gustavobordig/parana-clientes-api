using System.Text;
using Clientes.Application.Autenticacao;
using Clientes.Infrastructure.Autenticacao;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Clientes.Tests.Unit.Infrastructure;

public sealed class GeradorTokenJwtTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    private static readonly AutenticacaoOptions Opcoes = new()
    {
        Usuario = "moderador",
        Senha = "senha",
        ChaveAssinatura = "chave-de-teste-com-mais-de-trinta-e-dois-bytes",
        Emissor = "emissor-teste",
        Audiencia = "audiencia-teste",
        ExpiracaoMinutos = 60
    };

    private readonly GeradorTokenJwt _gerador = new(Options.Create(Opcoes), new FakeTimeProvider(Agora));

    private static TokenValidationParameters Parametros(string chave) => new()
    {
        ValidIssuer = Opcoes.Emissor,
        ValidAudience = Opcoes.Audiencia,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
        ValidateLifetime = false
    };

    [Fact]
    public void Gerar_Usuario_RetornaTokenBearerComExpiracaoConfigurada()
    {
        var token = _gerador.Gerar("moderador");

        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal(Agora.AddMinutes(60), token.ExpiraEm);
        Assert.Equal(Agora.AddMinutes(60).UtcDateTime, new JsonWebToken(token.AccessToken).ValidTo);
    }

    [Fact]
    public async Task Gerar_Usuario_TokenValidoComEmissorAudienciaEPapelDeModerador()
    {
        var token = _gerador.Gerar("Moderador");

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token.AccessToken, Parametros(Opcoes.ChaveAssinatura));

        Assert.True(resultado.IsValid);
        Assert.Equal("moderador", resultado.Claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal(Papeis.Moderador, resultado.Claims[AutenticacaoOptions.ClaimPapel]);
    }

    [Fact]
    public async Task Gerar_TokenValidadoComOutraChave_EhRejeitado()
    {
        var token = _gerador.Gerar("moderador");

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(
            token.AccessToken, Parametros("outra-chave-com-mais-de-trinta-e-dois-bytes!!"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Gerar_DuasVezes_CadaTokenTemIdentificadorProprio()
    {
        var primeiro = new JsonWebToken(_gerador.Gerar("moderador").AccessToken);
        var segundo = new JsonWebToken(_gerador.Gerar("moderador").AccessToken);

        Assert.NotEqual(primeiro.Id, segundo.Id);
    }
}
