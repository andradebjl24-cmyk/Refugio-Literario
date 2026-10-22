using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Comentario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Digite um comentário.")]
    [StringLength(500)]
    public string Texto { get; set; } = "";

    public DateTime Data { get; set; } = DateTime.Now;

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public int LivroId { get; set; }

    public Livro? Livro { get; set; }
}