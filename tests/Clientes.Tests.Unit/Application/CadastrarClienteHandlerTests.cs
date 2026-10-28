using Clientes.Application.Abstractions;
using Clientes.Application.Clientes.Cadastrar;
using Clientes.Application.Exceptions;
using Clientes.Domain.Common;
using Clientes.Domain.Entities;
using Clientes.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using ParanaBanco.Credito.Contracts;

namespace Clientes.Tests.Unit.Application;

public sealed class CadastrarClienteHandlerTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 28, 17, 0, 0, TimeSpan.Zero);

    private readonly IClienteRepository _repositorio = Substitute.For<IClienteRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPublicadorEventos _publicador = Substitute.For<IPublicadorEventos>();
    private readonly CadastrarClienteHandler _handler;

    public CadastrarClienteHandlerTests()
    {
        _handler = new CadastrarClienteHandler(
            _repositorio,
            _unitOfWork,
            _publicador,
            new FakeTimeProvider(Agora),
            NullLogger<CadastrarClienteHandler>.Instance);
    }

    private static CadastrarClienteCommand ComandoValido() =>
        new("Maria Silva", "529.982.247-25", "maria@exemplo.com", new DateOnly(1990, 5, 10), 5000m);

    [Fact]
    public async Task HandleAsync_DadosValidos_SalvaClienteEPublicaEvento()
    {
        _repositorio.ExisteCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(false);

        var resposta = await _handler.HandleAsync(ComandoValido(), CancellationToken.None);

        _repositorio.Received(1).Adicionar(Arg.Is<Cliente>(c => c.Id == resposta.Id));
        await _publicador.Received(1).PublicarAsync(
            Arg.Is<ClienteCadastrado>(e =>
                e.ClienteId == resposta.Id &&
                e.Nome == "Maria Silva" &&
                e.RendaMensal == 5000m &&
                e.OcorridoEm == Agora),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DadosValidos_RetornaCpfMascarado()
    {
        _repositorio.ExisteCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(false);

        var resposta = await _handler.HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal("***.982.247-**", resposta.Cpf);
    }

    [Fact]
    public async Task HandleAsync_DadosValidos_PublicaAntesDeSalvarParaEntrarNoOutbox()
    {
        _repositorio.ExisteCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(false);

        await _handler.HandleAsync(ComandoValido(), CancellationToken.None);

        Received.InOrder(() =>
        {
            _publicador.PublicarAsync(Arg.Any<ClienteCadastrado>(), Arg.Any<CancellationToken>());
            _unitOfWork.SalvarAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task HandleAsync_CpfJaCadastrado_LancaConflitoSemSalvarNemPublicar()
    {
        _repositorio.ExisteCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<CpfJaCadastradoException>(() =>
            _handler.HandleAsync(ComandoValido(), CancellationToken.None));

        _repositorio.DidNotReceive().Adicionar(Arg.Any<Cliente>());
        await _publicador.DidNotReceive().PublicarAsync(Arg.Any<ClienteCadastrado>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DadosInvalidos_LancaDomainExceptionSemConsultarBanco()
    {
        var comando = ComandoValido() with { Cpf = "000.000.000-00" };

        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(comando, CancellationToken.None));

        await _repositorio.DidNotReceive().ExisteCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SalvarAsync(Arg.Any<CancellationToken>());
    }
}
