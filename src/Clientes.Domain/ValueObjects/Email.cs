using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using Clientes.Domain.Common;

namespace Clientes.Domain.ValueObjects;

public sealed record Email
{
    public const int TamanhoMaximo = 254;

    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    public static Email Criar(string? valor) =>
        TentarCriar(valor, out var email) ? email : throw new DomainException("E-mail inválido.");

    public static bool TentarCriar(string? valor, [NotNullWhen(true)] out Email? email)
    {
        email = null;

        if (string.IsNullOrWhiteSpace(valor))
            return false;

        var normalizado = valor.Trim().ToLowerInvariant();

        if (normalizado.Length > TamanhoMaximo
            || !MailAddress.TryCreate(normalizado, out var endereco)
            || endereco.Address != normalizado
            || !endereco.Host.Contains('.'))
            return false;

        email = new Email(normalizado);
        return true;
    }

    public override string ToString() => Valor;
}
