using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Exemplar
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o tombo do exemplar.")]
    [StringLength(40)]
    public string Tombo { get; set; } = "";

    public StatusExemplar Status { get; set; } = StatusExemplar.Disponivel;

    public int LivroId { get; set; }

    public Livro? Livro { get; set; }

    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
}