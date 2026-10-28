using Clientes.Application.Abstractions;
using Clientes.Application.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Infrastructure.Persistence;

internal sealed class UnitOfWork(ClientesDbContext contexto) : IUnitOfWork
{
    private static readonly int[] ErrosDeChaveDuplicada = [2601, 2627];

    public async Task SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && ErrosDeChaveDuplicada.Contains(sql.Number))
        {
            throw new CpfJaCadastradoException();
        }
    }
}
