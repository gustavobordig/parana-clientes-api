using Clientes.Domain.ValueObjects;

namespace Clientes.Tests.Unit.Domain;

public sealed class EmailTests
{
    [Fact]
    public void Criar_EmailComEspacosEMaiusculas_Normaliza()
    {
        var email = Email.Criar("  Maria.Silva@Exemplo.com ");

        Assert.Equal("maria.silva@exemplo.com", email.Valor);
    }

    [Theory]
    [InlineData("maria.exemplo.com")]
    [InlineData("maria@")]
    [InlineData("@exemplo.com")]
    [InlineData("maria@exemplo")]
    [InlineData("Maria <maria@exemplo.com>")]
    [InlineData("")]
    [InlineData(null)]
    public void TentarCriar_EmailInvalido_RetornaFalso(string? entrada)
    {
        var valido = Email.TentarCriar(entrada, out var email);

        Assert.False(valido);
        Assert.Null(email);
    }

    [Fact]
    public void TentarCriar_EmailAcimaDoTamanhoMaximo_RetornaFalso()
    {
        var entrada = new string('a', Email.TamanhoMaximo) + "@exemplo.com";

        var valido = Email.TentarCriar(entrada, out _);

        Assert.False(valido);
    }
}
