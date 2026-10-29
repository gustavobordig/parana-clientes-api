using Clientes.Application.Abstractions;
using Clientes.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Clientes.Application.Autenticacao;

public sealed record AutenticarCommand(string? Usuario, string? Senha);

public sealed class AutenticarHandler(
    IVerificadorCredenciais verificador,
    IGeradorToken gerador,
    ILogger<AutenticarHandler> logger)
{
    public TokenAcesso Handle(AutenticarCommand comando)
    {
        if (string.IsNullOrWhiteSpace(comando.Usuario)
            || string.IsNullOrEmpty(comando.Senha)
            || !verificador.Verificar(comando.Usuario, comando.Senha))
        {
            logger.LogWarning("Tentativa de login recusada para o usuário {Usuario}", comando.Usuario);
            throw new CredenciaisInvalidasException();
        }

        logger.LogInformation("Usuário {Usuario} autenticado", comando.Usuario);
        return gerador.Gerar(comando.Usuario);
    }
}
