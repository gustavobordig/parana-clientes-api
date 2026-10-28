using Clientes.Application.Abstractions;
using Clientes.Infrastructure.Messaging;
using Clientes.Infrastructure.Observability;
using Clientes.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParanaBanco.Credito.Contracts;

namespace Clientes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuracao)
    {
        var sqlServer = configuracao.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer não configurada.");

        var rabbitMq = RabbitMqOptions.DaUrl(configuracao["RabbitMq:Url"]
            ?? throw new InvalidOperationException("RabbitMq:Url não configurada."));

        services.AddDbContext<ClientesDbContext>(options =>
            options.UseSqlServer(sqlServer, sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPublicadorEventos, PublicadorEventos>();
        services.AddScoped<CorrelationContext>();

        services.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<ClientesDbContext>(outbox =>
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
            });

            bus.UsingRabbitMq((contexto, rabbit) =>
            {
                rabbit.Host(rabbitMq.Host, rabbitMq.Porta, rabbitMq.VirtualHost, host =>
                {
                    host.Username(rabbitMq.Usuario);
                    host.Password(rabbitMq.Senha);
                });

                rabbit.Message<ClienteCadastrado>(m => m.SetEntityName("cliente-cadastrado"));

                rabbit.ConfigureEndpoints(contexto);
            });
        });

        services.AddHealthChecks().AddDbContextCheck<ClientesDbContext>("sqlserver");

        return services;
    }
}
