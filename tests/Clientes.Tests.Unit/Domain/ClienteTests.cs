using Clientes.Domain.Common;
using Clientes.Domain.Entities;

namespace Clientes.Tests.Unit.Domain;

public sealed class ClienteTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 28, 17, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(Agora.UtcDateTime);

    [Fact]
    public void Criar_DadosValidos_CriaClienteNormalizado()
    {
        var cliente = Cliente.Criar("  Maria Silva ", "529.982.247-25", "Maria@Exemplo.com", new DateOnly(1990, 5, 10), 5000m, Agora);

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("52998224725", cliente.Cpf.Valor);
        Assert.Equal("maria@exemplo.com", cliente.Email.Valor);
        Assert.Equal(5000m, cliente.RendaMensal);
        Assert.Equal(Agora, cliente.CriadoEm);
    }

    [Fact]
    public void Criar_RendaZero_CriaCliente()
    {
        var cliente = Cliente.Criar("Maria", "52998224725", "maria@exemplo.com", new DateOnly(1990, 5, 10), 0m, Agora);

        Assert.Equal(0m, cliente.RendaMensal);
    }

    [Fact]
    public void Criar_ExatamenteDezoitoAnosHoje_CriaCliente()
    {
        var cliente = Cliente.Criar("Maria", "52998224725", "maria@exemplo.com", Hoje.AddYears(-18), 1000m, Agora);

        Assert.Equal(Hoje.AddYears(-18), cliente.DataNascimento);
    }

    [Fact]
    public void Criar_DezoitoAnosAmanha_LancaDomainException()
    {
        var excecao = Assert.Throws<DomainException>(() =>
            Cliente.Criar("Maria", "52998224725", "maria@exemplo.com", Hoje.AddYears(-18).AddDays(1), 1000m, Agora));

        Assert.Contains("Cliente deve ter pelo menos 18 anos.", excecao.Erros);
    }

    [Fact]
    public void Criar_DataNascimentoNoFuturo_LancaDomainException()
    {
        var excecao = Assert.Throws<DomainException>(() =>
            Cliente.Criar("Maria", "52998224725", "maria@exemplo.com", Hoje.AddDays(1), 1000m, Agora));

        Assert.Contains("Data de nascimento inválida.", excecao.Erros);
    }

    [Fact]
    public void Criar_RendaNegativa_LancaDomainException()
    {
        var excecao = Assert.Throws<DomainException>(() =>
            Cliente.Criar("Maria", "52998224725", "maria@exemplo.com", new DateOnly(1990, 5, 10), -0.01m, Agora));

        Assert.Contains("Renda mensal não pode ser negativa.", excecao.Erros);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_NomeVazio_LancaDomainException(string? nome)
    {
        var excecao = Assert.Throws<DomainException>(() =>
            Cliente.Criar(nome, "52998224725", "maria@exemplo.com", new DateOnly(1990, 5, 10), 1000m, Agora));

        Assert.Contains("Nome é obrigatório.", excecao.Erros);
    }

    [Fact]
    public void Criar_NomeAcimaDoLimite_LancaDomainException()
    {
        var nome = new string('a', Cliente.NomeTamanhoMaximo + 1);

        Assert.Throws<DomainException>(() =>
            Cliente.Criar(nome, "52998224725", "maria@exemplo.com", new DateOnly(1990, 5, 10), 1000m, Agora));
    }

    [Fact]
    public void Criar_VariosCamposInvalidos_ReportaTodosOsErros()
    {
        var excecao = Assert.Throws<DomainException>(() =>
            Cliente.Criar("", "123", "invalido", default, -1m, Agora));

        Assert.Equal(5, excecao.Erros.Count);
    }

    [Theory]
    [InlineData("2000-10-28", "2026-10-28", 26)]
    [InlineData("2000-10-29", "2026-10-28", 25)]
    [InlineData("2000-02-29", "2026-02-28", 25)]
    [InlineData("2000-02-29", "2026-03-01", 26)]
    public void CalcularIdade_DatasDeAniversario_ConsideraSeJaFezAniversario(string nascimento, string hoje, int idadeEsperada)
    {
        var idade = Cliente.CalcularIdade(DateOnly.Parse(nascimento), DateOnly.Parse(hoje));

        Assert.Equal(idadeEsperada, idade);
    }
}
