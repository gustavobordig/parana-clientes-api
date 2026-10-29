namespace Clientes.Application.Abstractions;

public interface IVerificadorCredenciais
{
    bool Verificar(string usuario, string senha);
}
