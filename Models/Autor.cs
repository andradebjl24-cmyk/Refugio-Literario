using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Autor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do autor.")]
    [StringLength(150)]
    public string Nome { get; set; } = "";

    public ICollection<LivroAutor> LivrosAutores { get; set; } = new List<LivroAutor>();
}
