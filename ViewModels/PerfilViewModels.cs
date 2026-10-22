using System.ComponentModel.DataAnnotations;

namespace Biblioteca.ViewModels;

public class PerfilViewModel
{

    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(120)]
    public string Nome { get; set; } = "";

    // CPF será apenas exibido na tela.
    public string CPF { get; set; } = "";

    // E-mail será apenas exibido na tela.
    public string Email { get; set; } = "";

    // Avatar escolhido pelo usuário.
    public string? Avatar { get; set; }
}