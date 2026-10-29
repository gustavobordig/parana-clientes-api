using Clientes.Application.Abstractions;
using Clientes.Application.Autenticacao;
using Clientes.Application.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Clientes.Tests.Unit.Application;

public sealed class AutenticarHandlerTests
{
    private readonly IVerificadorCredenciais _verificador = Substitute.For<IVerificadorCredenciais>();
    private readonly IGeradorToken _gerador = Substitute.For<IGeradorToken>();
    private readonly AutenticarHandler _handler;

    public AutenticarHandlerTests()
    {
        _handler = new AutenticarHandler(_verificador, _gerador, NullLogger<AutenticarHandler>.Instance);
    }

    [Fact]
    public void Handle_CredenciaisValidas_RetornaTokenGerado()
    {
        var esperado = new TokenAcesso("token", "Bearer", DateTimeOffset.UtcNow.AddHours(1));
        _verificador.Verificar("moderador", "senha").Returns(true);
        _gerador.Gerar("moderador").Returns(esperado);

        var token = _handler.Handle(new AutenticarCommand("moderador", "senha"));

        Assert.Same(esperado, token);
    }

    [Fact]
    public void Handle_CredenciaisInvalidas_LancaExcecaoSemGerarToken()
    {
        _verificador.Verificar(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        Assert.Throws<CredenciaisInvalidasException>(() => _handler.Handle(new AutenticarCommand("moderador", "errada")));

        _gerador.DidNotReceive().Gerar(Arg.Any<string>());
    }

    [Theory]
    [InlineData(null, "senha")]
    [InlineData("", "senha")]
    [InlineData("   ", "senha")]
    [InlineData("moderador", null)]
    [InlineData("moderador", "")]
    public void Handle_UsuarioOuSenhaAusentes_LancaExcecaoSemConsultarCredenciais(string? usuario, string? senha)
    {
        Assert.Throws<CredenciaisInvalidasException>(() => _handler.Handle(new AutenticarCommand(usuario, senha)));

        _verificador.DidNotReceive().Verificar(Arg.Any<string>(), Arg.Any<string>());
    }
}
