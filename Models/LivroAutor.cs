namespace Biblioteca.Models;

/// <summary>Associação entre livros e autores (muitos para muitos).</summary>
public class LivroAutor
{
    public int LivroId { get; set; }
    public Livro? Livro { get; set; }

    public int AutorId { get; set; }
    public Autor? Autor { get; set; }
}
