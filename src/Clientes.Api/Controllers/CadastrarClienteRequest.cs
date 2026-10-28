using Clientes.Application.Clientes.Cadastrar;

namespace Clientes.Api.Controllers;

public sealed record CadastrarClienteRequest(
    string Nome,
    string Cpf,
    string Email,
    DateOnly DataNascimento,
    decimal RendaMensal)
{
    public CadastrarClienteCommand ParaComando() => new(Nome, Cpf, Email, DataNascimento, RendaMensal);
}
