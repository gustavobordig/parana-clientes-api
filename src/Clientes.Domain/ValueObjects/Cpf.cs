using System.Diagnostics.CodeAnalysis;
using Clientes.Domain.Common;

namespace Clientes.Domain.ValueObjects;

public sealed record Cpf
{
    private const int Tamanho = 11;

    public string Valor { get; }

    private Cpf(string valor) => Valor = valor;

    public string Mascarado => $"***.{Valor[3..6]}.{Valor[6..9]}-**";

    public static Cpf Criar(string? valor) =>
        TentarCriar(valor, out var cpf) ? cpf : throw new DomainException("CPF inválido.");

    public static bool TentarCriar(string? valor, [NotNullWhen(true)] out Cpf? cpf)
    {
        cpf = null;

        if (string.IsNullOrWhiteSpace(valor) || valor.Any(c => !char.IsDigit(c) && c is not '.' and not '-'))
            return false;

        var digitos = new string(valor.Where(char.IsDigit).ToArray());

        if (digitos.Length != Tamanho || digitos.Distinct().Count() == 1)
            return false;

        if (CalcularDigito(digitos, 9) != digitos[9] - '0' || CalcularDigito(digitos, 10) != digitos[10] - '0')
            return false;

        cpf = new Cpf(digitos);
        return true;
    }

    private static int CalcularDigito(string digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += (digitos[i] - '0') * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Mascarado;
}
