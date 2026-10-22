using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Editora
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da editora.")]
    [StringLength(150)]
    public string Nome { get; set; } = "";

    [StringLength(18)]
    public string CNPJ { get; set; } = "";

    [StringLength(100)]
    public string Cidade { get; set; } = "";

    [StringLength(2)]
    public string Estado { get; set; } = "";

    public ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
