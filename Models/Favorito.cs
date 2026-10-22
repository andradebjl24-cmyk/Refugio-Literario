namespace Biblioteca.Models;

public class Favorito
{
    public int Id { get; set; }

    // usuário que favoritou
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // livro favoritado
    public int LivroId { get; set; }
    public Livro? Livro { get; set; }
}