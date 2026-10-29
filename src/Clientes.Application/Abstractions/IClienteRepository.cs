using Clientes.Domain.Entities;
using Clientes.Domain.ValueObjects;

namespace Clientes.Application.Abstractions;

public interface IClienteRepository
{
    Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken cancellationToken);

    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Cliente> Clientes, int Total)> ListarAsync(int pagina, int tamanho, CancellationToken cancellationToken);

    void Adicionar(Cliente cliente);
}
