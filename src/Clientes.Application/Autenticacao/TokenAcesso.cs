namespace Clientes.Application.Autenticacao;

public sealed record TokenAcesso(string AccessToken, string TokenType, DateTimeOffset ExpiraEm);
