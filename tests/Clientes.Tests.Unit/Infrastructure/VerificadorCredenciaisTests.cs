using Clientes.Infrastructure.Autenticacao;
using Microsoft.Extensions.Options;

namespace Clientes.Tests.Unit.Infrastructure;

public sealed class VerificadorCredenciaisTests
{
    private readonly VerificadorCredenciais _verificador = new(Options.Create(new AutenticacaoOptions
    {
        Usuario = "moderador",
        Senha = "S3nha-Forte!"
    }));

    [Theory]
    [InlineData("moderador")]
    [InlineData("Moderador")]
    [InlineData("  moderador ")]
    public void Verificar_CredenciaisCorretas_RetornaVerdadeiroIgnorandoCaixaDoUsuario(string usuario)
    {
        var valido = _verificador.Verificar(usuario, "S3nha-Forte!");

        Assert.True(valido);
    }

    [Theory]
    [InlineData("moderador", "s3nha-forte!")]
    [InlineData("moderador", "S3nha-Forte")]
    [InlineData("moderador", "")]
    [InlineData("admin", "S3nha-Forte!")]
    [InlineData("", "")]
    public void Verificar_CredenciaisIncorretas_RetornaFalso(string usuario, string senha)
    {
        var valido = _verificador.Verificar(usuario, senha);

        Assert.False(valido);
    }
}
