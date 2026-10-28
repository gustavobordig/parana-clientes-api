using Clientes.Application.Abstractions;
using Clientes.Application.Clientes.Obter;
using Clientes.Domain.Entities;
using NSubstitute;

namespace Clientes.Tests.Unit.Application;

public sealed class ObterClienteHandlerTests
{
    private readonly IClienteRepository _repositorio = Substitute.For<IClienteRepository>();
    private readonly ObterClienteHandler _handler;

    public ObterClienteHandlerTests()
    {
        _handler = new ObterClienteHandler(_repositorio);
    }

    [Fact]
    public async Task HandleAsync_ClienteExistente_RetornaDadosComCpfMascarado()
    {
        var cliente = Cliente.Criar("Maria", "52998224725", "maria@exemplo.com", new DateOnly(1990, 5, 10), 5000m, DateTimeOffset.UtcNow);
        _repositorio.ObterPorIdAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(cliente);

        var resposta = await _handler.HandleAsync(cliente.Id, CancellationToken.None);

        Assert.NotNull(resposta);
        Assert.Equal(cliente.Id, resposta.Id);
        Assert.Equal("***.982.247-**", resposta.Cpf);
    }

    [Fact]
    public async Task HandleAsync_ClienteInexistente_RetornaNulo()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Cliente?)null);

        var resposta = await _handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(resposta);
    }
}
