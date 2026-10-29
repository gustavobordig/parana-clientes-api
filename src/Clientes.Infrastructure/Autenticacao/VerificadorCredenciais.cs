using System.Security.Cryptography;
using System.Text;
using Clientes.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Clientes.Infrastructure.Autenticacao;

internal sealed class VerificadorCredenciais(IOptions<AutenticacaoOptions> opcoes) : IVerificadorCredenciais
{
    public bool Verificar(string usuario, string senha)
    {
        var configurado = opcoes.Value;

        var usuarioConfere = IguaisEmTempoConstante(usuario.Trim().ToLowerInvariant(), configurado.Usuario.Trim().ToLowerInvariant());
        var senhaConfere = IguaisEmTempoConstante(senha, configurado.Senha);

        return usuarioConfere & senhaConfere;
    }

    private static bool IguaisEmTempoConstante(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(a)),
            SHA256.HashData(Encoding.UTF8.GetBytes(b)));
}
