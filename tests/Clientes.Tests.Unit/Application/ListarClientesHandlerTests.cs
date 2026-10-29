using Clientes.Application.Abstractions;
using Clientes.Application.Clientes.Listar;
using Clientes.Domain.Entities;
using NSubstitute;

namespace Clientes.Tests.Unit.Application;

public sealed class ListarClientesHandlerTests
{
    private readonly IClienteRepository _repositorio = Substitute.For<IClienteRepository>();
    private readonly ListarClientesHandler _handler;

    public ListarClientesHandlerTests()
    {
        _handler = new ListarClientesHandler(_repositorio);
    }

    private static Cliente NovoCliente(string cpf) =>
        Cliente.Criar("Maria", cpf, "maria@exemplo.com", new DateOnly(1990, 5, 10), 5000m, DateTimeOffset.UtcNow);

    [Fact]
    public async Task HandleAsync_PaginaComClientes_RetornaItensComCpfMascaradoETotais()
    {
        var clientes = new List<Cliente> { NovoCliente("52998224725"), NovoCliente("11144477735") };
        _repositorio.ListarAsync(2, 2, Arg.Any<CancellationToken>()).Returns((clientes, 5));

        var pagina = await _handler.HandleAsync(2, 2, CancellationToken.None);

        Assert.Equal(2, pagina.Itens.Count);
        Assert.All(pagina.Itens, c => Assert.StartsWith("***.", c.Cpf));
        Assert.Equal(2, pagina.Pagina);
        Assert.Equal(5, pagina.Total);
        Assert.Equal(3, pagina.TotalPaginas);
    }

    [Fact]
    public async Task HandleAsync_SemClientes_RetornaPaginaVazia()
    {
        _repositorio.ListarAsync(1, 20, Arg.Any<CancellationToken>()).Returns((new List<Cliente>(), 0));

        var pagina = await _handler.HandleAsync(1, 20, CancellationToken.None);

        Assert.Empty(pagina.Itens);
        Assert.Equal(0, pagina.TotalPaginas);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, ListarClientesHandler.TamanhoMaximo + 1)]
    public async Task HandleAsync_PaginacaoInvalida_LancaExcecaoSemConsultarBanco(int pagina, int tamanho)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _handler.HandleAsync(pagina, tamanho, CancellationToken.None));

        await _repositorio.DidNotReceive().ListarAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
