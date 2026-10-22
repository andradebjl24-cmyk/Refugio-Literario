namespace Biblioteca.Models;

public enum TipoUsuario
{
    Administrador = 1,
    UsuarioComum = 2
}

public enum StatusExemplar
{
    Disponivel = 1,
    Emprestado = 2,
    Inativo = 4
}

public enum StatusEmprestimo
{
    Solicitado = 0,
    Ativo = 1,
    Devolvido = 2,
    Recusado = 3
}
