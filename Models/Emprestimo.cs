namespace Biblioteca.Models;

public class Emprestimo
{
    public int Id { get; set; }

    // exemplar emprestado
    public int ExemplarId { get; set; }
    public Exemplar? Exemplar { get; set; }

    // usuário que realizou o empréstimo
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // datas do empréstimo
    public DateTime DataEmprestimo { get; set; } = DateTime.Today;

    public DateTime DataPrevistaDevolucao { get; set; }
        = DateTime.Today.AddDays(15);

    public DateTime? DataDevolucao { get; set; }

    // situação atual do empréstimo
    public StatusEmprestimo Status { get; set; }
        = StatusEmprestimo.Solicitado;
}