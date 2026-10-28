namespace Clientes.Application.Exceptions;

public sealed class CpfJaCadastradoException() : Exception("Já existe um cliente cadastrado com este CPF.");
