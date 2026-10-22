using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Nome { get; set; } = "";

    [Required]
    [StringLength(14)]
    public string CPF { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required]
    public string SenhaHash { get; set; } = "";

    public TipoUsuario TipoUsuario { get; set; }
        = TipoUsuario.UsuarioComum;

    public bool Ativo { get; set; } = true;

    public string? Avatar { get; set; }

    public ICollection<Emprestimo> Emprestimos { get; set; }
        = new List<Emprestimo>();
}
