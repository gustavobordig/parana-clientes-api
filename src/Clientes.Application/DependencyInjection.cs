using Clientes.Application.Autenticacao;
using Clientes.Application.Clientes.Cadastrar;
using Clientes.Application.Clientes.Listar;
using Clientes.Application.Clientes.Obter;
using Microsoft.Extensions.DependencyInjection;

namespace Clientes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CadastrarClienteHandler>();
        services.AddScoped<ObterClienteHandler>();
        services.AddScoped<ListarClientesHandler>();
        services.AddScoped<AutenticarHandler>();
        return services;
    }
}
