using Clientes.Domain.Common;
using Clientes.Domain.ValueObjects;

namespace Clientes.Tests.Unit.Domain;

public sealed class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData(" 529.982.247-25 ")]
    public void Criar_CpfValidoComOuSemMascara_ArmazenaSomenteDigitos(string entrada)
    {
        var cpf = Cpf.Criar(entrada.Trim());

        Assert.Equal("52998224725", cpf.Valor);
    }

    [Theory]
    [InlineData("52998224724")]
    [InlineData("52998224715")]
    [InlineData("11111111111")]
    [InlineData("5299822472")]
    [InlineData("529982247250")]
    [InlineData("529.982.247-2a")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TentarCriar_CpfInvalido_RetornaFalso(string? entrada)
    {
        var valido = Cpf.TentarCriar(entrada, out var cpf);

        Assert.False(valido);
        Assert.Null(cpf);
    }

    [Fact]
    public void Criar_CpfInvalido_LancaDomainException()
    {
        var excecao = Assert.Throws<DomainException>(() => Cpf.Criar("12345678900"));

        Assert.Contains("CPF inválido.", excecao.Erros);
    }

    [Fact]
    public void Mascarado_CpfValido_OcultaPrimeirosEUltimosDigitos()
    {
        var cpf = Cpf.Criar("52998224725");

        var mascarado = cpf.Mascarado;

        Assert.Equal("***.982.247-**", mascarado);
    }

    [Fact]
    public void ToString_CpfValido_NaoExpoeCpfCompleto()
    {
        var cpf = Cpf.Criar("52998224725");

        var texto = $"{cpf}";

        Assert.DoesNotContain("52998224725", texto);
    }

    [Fact]
    public void Equals_MesmoCpfComFormatosDiferentes_SaoIguais()
    {
        var semMascara = Cpf.Criar("52998224725");
        var comMascara = Cpf.Criar("529.982.247-25");

        Assert.Equal(semMascara, comMascara);
    }
}
