using System.ComponentModel.DataAnnotations;
using Biblioteca.Models;

namespace Biblioteca.ViewModels;

public class AdminUsuarioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120)]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(14)]
    public string CPF { get; set; } = "";

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }

    public TipoUsuario TipoUsuario { get; set; } = TipoUsuario.UsuarioComum;
    public bool Ativo { get; set; } = true;
}
