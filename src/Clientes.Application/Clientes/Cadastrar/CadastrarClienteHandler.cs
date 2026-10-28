using Clientes.Application.Abstractions;
using Clientes.Application.Exceptions;
using Clientes.Domain.Entities;
using Microsoft.Extensions.Logging;
using ParanaBanco.Credito.Contracts;

namespace Clientes.Application.Clientes.Cadastrar;

public sealed record CadastrarClienteCommand(
    string? Nome,
    string? Cpf,
    string? Email,
    DateOnly DataNascimento,
    decimal RendaMensal);

public sealed class CadastrarClienteHandler(
    IClienteRepository repositorio,
    IUnitOfWork unitOfWork,
    IPublicadorEventos publicador,
    TimeProvider relogio,
    ILogger<CadastrarClienteHandler> logger)
{
    public async Task<ClienteResponse> HandleAsync(CadastrarClienteCommand comando, CancellationToken cancellationToken)
    {
        var agora = relogio.GetUtcNow();

        var cliente = Cliente.Criar(
            comando.Nome,
            comando.Cpf,
            comando.Email,
            comando.DataNascimento,
            comando.RendaMensal,
            agora);

        if (await repositorio.ExisteCpfAsync(cliente.Cpf, cancellationToken))
            throw new CpfJaCadastradoException();

        repositorio.Adicionar(cliente);

        await publicador.PublicarAsync(
            new ClienteCadastrado(cliente.Id, cliente.Nome, cliente.RendaMensal, agora),
            cancellationToken);

        await unitOfWork.SalvarAsync(cancellationToken);

        logger.LogInformation("Cliente {ClienteId} cadastrado com CPF {Cpf}", cliente.Id, cliente.Cpf.Mascarado);

        return ClienteResponse.De(cliente);
    }
}
