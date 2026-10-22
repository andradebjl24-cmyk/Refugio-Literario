using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Livro
{
    public int Id { get; set; }

    [Required]
    [StringLength(180)]
    public string Titulo { get; set; } = "";

    [Required]
    [StringLength(20)]
    public string ISBN { get; set; } = "";

    [Range(0, 9999, ErrorMessage = "Informe um ano de publicação válido.")]
    public int AnoPublicacao { get; set; }

    public bool Destaque { get; set; }

    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public ICollection<Categoria> Subcategorias { get; set; }
    = new List<Categoria>();

    public int EditoraId { get; set; }

    public Editora? Editora { get; set; }

    public string? Imagem { get; set; }

    public ICollection<Exemplar> Exemplares { get; set; }
        = new List<Exemplar>();

    public ICollection<LivroAutor> LivrosAutores { get; set; }
        = new List<LivroAutor>();

    public ICollection<Comentario> Comentarios { get; set; }
    = new List<Comentario>();

    [StringLength(3000)]
    public string? Sinopse { get; set; }
}