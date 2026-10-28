using Clientes.Domain.Entities;

namespace Clientes.Application.Clientes;

public sealed record ClienteResponse(
    Guid Id,
    string Nome,
    string Cpf,
    string Email,
    DateOnly DataNascimento,
    decimal RendaMensal,
    DateTimeOffset CriadoEm)
{
    public static ClienteResponse De(Cliente cliente) => new(
        cliente.Id,
        cliente.Nome,
        cliente.Cpf.Mascarado,
        cliente.Email.Valor,
        cliente.DataNascimento,
        cliente.RendaMensal,
        cliente.CriadoEm);
}
