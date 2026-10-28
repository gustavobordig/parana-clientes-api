namespace Clientes.Domain.Common;

public sealed class DomainException : Exception
{
    public IReadOnlyList<string> Erros { get; }

    public DomainException(string erro) : this([erro])
    {
    }

    public DomainException(IReadOnlyList<string> erros) : base(string.Join(" ", erros))
    {
        Erros = erros;
    }
}
