using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clientes.Infrastructure.Persistence;

internal sealed class ClientesDbContextFactory : IDesignTimeDbContextFactory<ClientesDbContext>
{
    public ClientesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseSqlServer("Server=localhost;Database=clientes_db;Trusted_Connection=False;")
            .Options;

        return new ClientesDbContext(options);
    }
}
