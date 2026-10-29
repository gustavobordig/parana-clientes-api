using Clientes.Application.Autenticacao;

namespace Clientes.Application.Abstractions;

public interface IGeradorToken
{
    TokenAcesso Gerar(string usuario);
}
