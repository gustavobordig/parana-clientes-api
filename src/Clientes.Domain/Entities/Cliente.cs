using Clientes.Domain.Common;
using Clientes.Domain.ValueObjects;

namespace Clientes.Domain.Entities;

public sealed class Cliente
{
    public const int IdadeMinima = 18;
    public const int NomeTamanhoMaximo = 150;

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public DateOnly DataNascimento { get; private set; }

    public decimal RendaMensal { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    private Cliente()
    {
    }

    public static Cliente Criar(
        string? nome,
        string? cpf,
        string? email,
        DateOnly dataNascimento,
        decimal rendaMensal,
        DateTimeOffset agora)
    {
        var erros = new List<string>();
        nome = nome?.Trim();

        if (string.IsNullOrWhiteSpace(nome))
            erros.Add("Nome é obrigatório.");
        else if (nome.Length > NomeTamanhoMaximo)
            erros.Add($"Nome deve ter no máximo {NomeTamanhoMaximo} caracteres.");

        if (!Cpf.TentarCriar(cpf, out var cpfValido))
            erros.Add("CPF inválido.");

        if (!Email.TentarCriar(email, out var emailValido))
            erros.Add("E-mail inválido.");

        var hoje = DateOnly.FromDateTime(agora.UtcDateTime);
        if (dataNascimento == default || dataNascimento > hoje)
            erros.Add("Data de nascimento inválida.");
        else if (CalcularIdade(dataNascimento, hoje) < IdadeMinima)
            erros.Add($"Cliente deve ter pelo menos {IdadeMinima} anos.");

        if (rendaMensal < 0)
            erros.Add("Renda mensal não pode ser negativa.");

        if (erros.Count > 0)
            throw new DomainException(erros);

        return new Cliente
        {
            Id = Guid.CreateVersion7(agora),
            Nome = nome!,
            Cpf = cpfValido!,
            Email = emailValido!,
            DataNascimento = dataNascimento,
            RendaMensal = rendaMensal,
            CriadoEm = agora
        };
    }

    internal static int CalcularIdade(DateOnly dataNascimento, DateOnly hoje)
    {
        var idade = hoje.Year - dataNascimento.Year;
        return dataNascimento > hoje.AddYears(-idade) ? idade - 1 : idade;
    }
}
