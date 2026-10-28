using Clientes.Domain.Entities;
using Clientes.Domain.ValueObjects;

namespace Clientes.Application.Abstractions;

public interface IClienteRepository
{
    Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken cancellationToken);

    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    void Adicionar(Cliente cliente);
}
