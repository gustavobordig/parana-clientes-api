using Clientes.Application.Abstractions;

namespace Clientes.Application.Clientes.Listar;

public sealed record PaginaResponse<T>(IReadOnlyList<T> Itens, int Pagina, int Tamanho, int Total)
{
    public int TotalPaginas => Tamanho == 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);
}

public sealed class ListarClientesHandler(IClienteRepository repositorio)
{
    public const int TamanhoMaximo = 100;

    public async Task<PaginaResponse<ClienteResponse>> HandleAsync(int pagina, int tamanho, CancellationToken cancellationToken)
    {
        if (pagina < 1)
            throw new ArgumentOutOfRangeException(nameof(pagina), "A página começa em 1.");

        if (tamanho is < 1 or > TamanhoMaximo)
            throw new ArgumentOutOfRangeException(nameof(tamanho), $"O tamanho da página deve ficar entre 1 e {TamanhoMaximo}.");

        var (clientes, total) = await repositorio.ListarAsync(pagina, tamanho, cancellationToken);

        return new PaginaResponse<ClienteResponse>(
            clientes.Select(ClienteResponse.De).ToList(),
            pagina,
            tamanho,
            total);
    }
}
