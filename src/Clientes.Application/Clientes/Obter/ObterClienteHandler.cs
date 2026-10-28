using Clientes.Application.Abstractions;

namespace Clientes.Application.Clientes.Obter;

public sealed class ObterClienteHandler(IClienteRepository repositorio)
{
    public async Task<ClienteResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await repositorio.ObterPorIdAsync(id, cancellationToken);
        return cliente is null ? null : ClienteResponse.De(cliente);
    }
}
