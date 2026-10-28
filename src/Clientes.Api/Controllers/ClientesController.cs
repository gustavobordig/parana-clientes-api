using Asp.Versioning;
using Clientes.Application.Clientes;
using Clientes.Application.Clientes.Cadastrar;
using Clientes.Application.Clientes.Obter;
using Microsoft.AspNetCore.Mvc;

namespace Clientes.Api.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/clientes")]
[Produces("application/json")]
public sealed class ClientesController(
    CadastrarClienteHandler cadastrarCliente,
    ObterClienteHandler obterCliente) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Cadastra um cliente e publica o evento ClienteCadastrado.")]
    [EndpointDescription("A proposta de crédito e os cartões são gerados de forma assíncrona após o cadastro.")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(
        CadastrarClienteRequest request,
        ApiVersion versao,
        CancellationToken cancellationToken)
    {
        var cliente = await cadastrarCliente.HandleAsync(request.ParaComando(), cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = cliente.Id, version = versao.ToString() },
            cliente);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Consulta um cliente pelo id.")]
    [EndpointDescription("O CPF é retornado mascarado.")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await obterCliente.HandleAsync(id, cancellationToken);
        return cliente is null ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Cliente não encontrado.") : Ok(cliente);
    }
}
