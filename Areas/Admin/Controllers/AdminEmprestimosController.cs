using System.Data;
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class AdminEmprestimosController : Controller
{
    private readonly BibliotecaContext db;

    public AdminEmprestimosController(BibliotecaContext db)
    {
        this.db = db;
    }

    public async Task<IActionResult> Index()
    {
        var emprestimos = await db.Emprestimos
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Include(x => x.Exemplar)
                .ThenInclude(x => x!.Livro)
            .Where(x =>
                x.Status == StatusEmprestimo.Solicitado ||
                x.Status == StatusEmprestimo.Ativo)
            .OrderBy(x => x.Status == StatusEmprestimo.Solicitado ? 0 : 1)
            .ThenBy(x => x.DataPrevistaDevolucao)
            .ToListAsync();

        return View(emprestimos);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CarregarOpcoes();

        return View(new Emprestimo
        {
            DataEmprestimo = DateTime.Today,
            DataPrevistaDevolucao = DateTime.Today.AddDays(15)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Emprestimo model)
    {
        ModelState.Remove(nameof(Emprestimo.Usuario));
        ModelState.Remove(nameof(Emprestimo.Exemplar));

        if (model.DataEmprestimo.Date > DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.DataEmprestimo),
                "A data do empréstimo não pode estar no futuro.");
        }

        if (model.DataPrevistaDevolucao.Date < model.DataEmprestimo.Date)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.DataPrevistaDevolucao),
                "A devolução prevista não pode ser anterior à data do empréstimo.");
        }

        if (!ModelState.IsValid)
        {
            await CarregarOpcoes(model.UsuarioId, model.ExemplarId);
            return View(model);
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var exemplar = await db.Exemplares
            .FirstOrDefaultAsync(x => x.Id == model.ExemplarId);

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(x => x.Id == model.UsuarioId);

        if (exemplar == null)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.ExemplarId),
                "Selecione um exemplar válido.");
        }
        else if (exemplar.Status != StatusExemplar.Disponivel)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.ExemplarId),
                "Este exemplar não está disponível para empréstimo.");
        }

        if (usuario == null || !usuario.Ativo)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.UsuarioId),
                "Selecione um usuário ativo.");
        }

        var emprestimoAtivo = await db.Emprestimos.AnyAsync(x =>
            x.ExemplarId == model.ExemplarId &&
            x.Status == StatusEmprestimo.Ativo);

        if (emprestimoAtivo)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.ExemplarId),
                "Este exemplar já possui um empréstimo ativo.");
        }

        // A regra é por livro, não apenas pelo exemplar selecionado.
        var pedidoDuplicado = exemplar != null && await db.Emprestimos.AnyAsync(x =>
            x.UsuarioId == model.UsuarioId &&
            x.Exemplar != null &&
            x.Exemplar.LivroId == exemplar.LivroId &&
            (x.Status == StatusEmprestimo.Solicitado ||
             x.Status == StatusEmprestimo.Ativo));

        if (pedidoDuplicado)
        {
            ModelState.AddModelError(
                nameof(Emprestimo.ExemplarId),
                "Este usuário já possui uma solicitação pendente ou um empréstimo ativo deste livro, mesmo que seja outra cópia.");
        }

        if (!ModelState.IsValid)
        {
            await CarregarOpcoes(model.UsuarioId, model.ExemplarId);
            return View(model);
        }

        model.Status = StatusEmprestimo.Ativo;
        model.DataDevolucao = null;

        exemplar!.Status = StatusExemplar.Emprestado;
        db.Emprestimos.Add(model);

        try
        {
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Mensagem"] = "Empréstimo registrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            db.ChangeTracker.Clear();

            await CarregarOpcoes(model.UsuarioId, model.ExemplarId);

            ModelState.AddModelError(
                "",
                "Não foi possível registrar o empréstimo. Atualize a página e confira se o exemplar continua disponível.");

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int id)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var emprestimo = await db.Emprestimos
            .Include(x => x.Exemplar)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (emprestimo == null)
            return NotFound();

        if (emprestimo.Status != StatusEmprestimo.Solicitado)
        {
            TempData["Erro"] = "Esta solicitação já foi processada.";
            return RedirectToAction(nameof(Index));
        }

        // Uma solicitação pode ter ficado pendente enquanto outro empréstimo
        // do mesmo livro foi ativado para esse usuário. Revalidar na aprovação.
        var outroEmprestimoDoMesmoLivro = emprestimo.Exemplar != null &&
            await db.Emprestimos.AnyAsync(x =>
                x.Id != emprestimo.Id &&
                x.UsuarioId == emprestimo.UsuarioId &&
                x.Exemplar != null &&
                x.Exemplar.LivroId == emprestimo.Exemplar.LivroId &&
                (x.Status == StatusEmprestimo.Solicitado ||
                 x.Status == StatusEmprestimo.Ativo));

        if (outroEmprestimoDoMesmoLivro)
        {
            TempData["Erro"] =
                "Não é possível aprovar: este usuário já possui outra solicitação pendente ou um empréstimo ativo deste livro. Recuse esta solicitação se ela não for mais necessária.";
            await transaction.RollbackAsync();
            return RedirectToAction(nameof(Index));
        }

        if (emprestimo.Exemplar == null ||
            emprestimo.Exemplar.Status != StatusExemplar.Disponivel)
        {
            emprestimo.Status = StatusEmprestimo.Recusado;

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Erro"] =
                "O exemplar não está mais disponível. A solicitação foi recusada.";

            return RedirectToAction(nameof(Index));
        }

        emprestimo.Status = StatusEmprestimo.Ativo;
        emprestimo.DataEmprestimo = DateTime.Today;
        emprestimo.DataPrevistaDevolucao = DateTime.Today.AddDays(15);
        emprestimo.DataDevolucao = null;
        emprestimo.Exemplar.Status = StatusExemplar.Emprestado;

        try
        {
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Mensagem"] =
                "Solicitação aprovada e empréstimo ativado.";
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            TempData["Erro"] =
                "Não foi possível aprovar a solicitação. Verifique a disponibilidade do exemplar e tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recusar(int id)
    {
        var emprestimo = await db.Emprestimos.FindAsync(id);

        if (emprestimo == null)
            return NotFound();

        if (emprestimo.Status != StatusEmprestimo.Solicitado)
        {
            TempData["Erro"] =
                "Somente solicitações pendentes podem ser recusadas.";

            return RedirectToAction(nameof(Index));
        }

        emprestimo.Status = StatusEmprestimo.Recusado;

        await db.SaveChangesAsync();

        TempData["Mensagem"] = "Solicitação recusada.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int id)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var emprestimo = await db.Emprestimos
            .Include(x => x.Exemplar)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (emprestimo == null)
            return NotFound();

        if (emprestimo.Status != StatusEmprestimo.Ativo)
        {
            TempData["Erro"] =
                "Somente empréstimos ativos podem ter a devolução registrada.";

            return RedirectToAction(nameof(Index));
        }

        emprestimo.Status = StatusEmprestimo.Devolvido;
        emprestimo.DataDevolucao = DateTime.Today;

        if (emprestimo.Exemplar != null &&
            emprestimo.Exemplar.Status == StatusExemplar.Emprestado)
        {
            emprestimo.Exemplar.Status = StatusExemplar.Disponivel;
        }

        try
        {
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Mensagem"] = "Livro devolvido com sucesso.";
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            TempData["Erro"] =
                "Não foi possível registrar a devolução. Atualize a página e tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarOpcoes(
        int? usuarioId = null,
        int? exemplarId = null)
    {
        ViewBag.Usuarios = await db.Usuarios
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        ViewBag.Exemplares = await db.Exemplares
            .AsNoTracking()
            .Include(x => x.Livro)
            .Where(x =>
                x.Status == StatusExemplar.Disponivel ||
                x.Id == exemplarId)
            .OrderBy(x => x.Livro!.Titulo)
            .ThenBy(x => x.Tombo)
            .ToListAsync();
    }
}
