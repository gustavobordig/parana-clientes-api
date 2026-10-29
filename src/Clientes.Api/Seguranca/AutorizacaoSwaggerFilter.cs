using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Clientes.Api.Seguranca;

internal sealed class AutorizacaoSwaggerFilter : IOperationFilter
{
    public const string Esquema = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var exigeAutenticacao = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any()
            && !context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();

        if (!exigeAutenticacao)
            return;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(Esquema, context.Document)] = []
        });
    }
}
