using Clientes.Application.Abstractions;
using Clientes.Domain.Entities;
using Clientes.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Infrastructure.Persistence;

internal sealed class ClienteRepository(ClientesDbContext contexto) : IClienteRepository
{
    public Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken cancellationToken) =>
        contexto.Clientes.AnyAsync(c => c.Cpf == cpf, cancellationToken);

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        contexto.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Adicionar(Cliente cliente) => contexto.Clientes.Add(cliente);
}
