using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Clientes.Infrastructure.Autenticacao;

public sealed class AutenticacaoOptions
{
    public const string Secao = "Autenticacao";
    public const int TamanhoMinimoChaveEmBytes = 32;
    public const string ClaimPapel = "role";

    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string ChaveAssinatura { get; set; } = string.Empty;
    public string Emissor { get; set; } = "parana-clientes-api";
    public string Audiencia { get; set; } = "parana-credito";
    public int ExpiracaoMinutos { get; set; } = 60;

    public bool CredenciaisConfiguradas => !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrEmpty(Senha);

    public bool ChaveValida => Encoding.UTF8.GetByteCount(ChaveAssinatura) >= TamanhoMinimoChaveEmBytes;

    public SymmetricSecurityKey CriarChave() => new(Encoding.UTF8.GetBytes(ChaveAssinatura));
}
