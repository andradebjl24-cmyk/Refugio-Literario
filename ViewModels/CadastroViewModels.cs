using System.ComponentModel.DataAnnotations;

namespace Biblioteca.ViewModels;

public class CadastroViewModel
{
    // NOME
    
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = "";

    
    // CPF

    [Required(ErrorMessage = "Informe seu CPF.")]
    [RegularExpression(@"^(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})$",
    ErrorMessage = "Informe um CPF com 11 dígitos, com ou sem pontuação.")]
    public string CPF { get; set; } = "";


    // E-MAIL
    
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    public string Email { get; set; } = "";


    // SENHA
    
    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = "";

    // CONFIRMAÇÃO DA SENHA
    
    [Required(ErrorMessage = "Confirme sua senha.")]
    [DataType(DataType.Password)]
    [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
    public string ConfirmarSenha { get; set; } = "";
}