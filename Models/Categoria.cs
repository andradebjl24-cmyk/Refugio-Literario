using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(250)]
    public string Nome { get; set; } = "";

    // Define se aparece na página inicial.
    public bool ExibirNaHome { get; set; } = false;

    // Categoria principal, quando houver.
    public int? CategoriaPaiId { get; set; }

    public Categoria? CategoriaPai { get; set; }

    // Categorias filhas.
    public ICollection<Categoria> Subcategorias { get; set; }
        = new List<Categoria>();

    // Livros que usam esta categoria como categoria principal.
    public ICollection<Livro> Livros { get; set; }
        = new List<Livro>();

    // Livros associados a esta categoria como subcategoria.
    public ICollection<Livro> LivrosComSubcategoria { get; set; }
        = new List<Livro>();
}