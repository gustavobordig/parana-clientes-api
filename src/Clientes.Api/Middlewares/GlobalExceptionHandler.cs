using Clientes.Application.Exceptions;
using Clientes.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Clientes.Api.Middlewares;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken cancellationToken)
    {
        var problema = exception switch
        {
            DomainException dominio => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dados inválidos.",
                Extensions = { ["erros"] = dominio.Erros }
            },
            CpfJaCadastradoException conflito => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Cliente já cadastrado.",
                Detail = conflito.Message
            },
            CredenciaisInvalidasException credenciais => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Não autorizado.",
                Detail = credenciais.Message
            },
            ArgumentOutOfRangeException argumento => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Parâmetro inválido.",
                Detail = argumento.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno."
            }
        };

        if (problema.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro não tratado ao processar {Path}", http.Request.Path);

        http.Response.StatusCode = problema.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = exception,
            ProblemDetails = problema
        });
    }
}
