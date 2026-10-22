using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class AdminExemplaresController : Controller
{
    private readonly BibliotecaContext db;

    public AdminExemplaresController(BibliotecaContext db)
    {
        this.db = db;
    }

    // LISTAGEM
    
    public async Task<IActionResult> Index()
    {
        var exemplares = await db.Exemplares
            .Include(x => x.Livro)
            .OrderBy(x => x.Livro!.Titulo)
            .ThenBy(x => x.Tombo)
            .ToListAsync();

        return View(exemplares);
    }

    // CRIAR
    
    public async Task<IActionResult> Create()
    {
        await CarregarLivros();

        return View(new Exemplar());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exemplar m)
    {
        if (!ModelState.IsValid)
        {
            await CarregarLivros();

            return View(m);
        }

        m.Tombo = m.Tombo.Trim();
        m.Status = StatusExemplar.Disponivel;

        try
        {
            db.Exemplares.Add(m);
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Exemplar cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(m.Tombo), "Não foi possível salvar. Confira se o livro existe e se o tombo já está cadastrado para ele.");
            await CarregarLivros();
            return View(m);
        }
    }

    // EDITAR
    
    public async Task<IActionResult> Edit(int id)
    {
        var exemplar = await db.Exemplares
            .FindAsync(id);

        if (exemplar == null)
        {
            return NotFound();
        }

        await CarregarLivros();

        return View(exemplar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exemplar m)
    {
        if (id != m.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await CarregarLivros();

            return View(m);
        }

        var existente = await db.Exemplares
            .Include(x => x.Emprestimos)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (existente == null) return NotFound();

        var possuiEmprestimoAtivo = existente.Emprestimos.Any(x => x.Status == StatusEmprestimo.Ativo);
        if (possuiEmprestimoAtivo && m.Status != StatusExemplar.Emprestado)
        {
            ModelState.AddModelError(nameof(m.Status), "Não é possível alterar o status enquanto houver um empréstimo ativo.");
            await CarregarLivros();
            return View(m);
        }

        existente.LivroId = m.LivroId;
        existente.Tombo = m.Tombo.Trim();
        if (!possuiEmprestimoAtivo)
            existente.Status = m.Status;

        try
        {
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Exemplar atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível salvar. Verifique se o tombo já existe para este livro.");
            await CarregarLivros();
            return View(m);
        }
    }

    // DETALHES
    
    public async Task<IActionResult> Details(int id)
    {
        var exemplar = await db.Exemplares
            .Include(x => x.Livro)
            .Include(x => x.Emprestimos)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (exemplar == null)
        {
            return NotFound();
        }

        return View(exemplar);
    }

    // EXCLUIR
    
    public async Task<IActionResult> Delete(int id)
    {
        var exemplar = await db.Exemplares
            .Include(x => x.Livro)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (exemplar == null)
        {
            return NotFound();
        }

        return View(exemplar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteOk(int id)
    {
        var exemplar = await db.Exemplares
            .Include(x => x.Emprestimos)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (exemplar != null)
        {
            if (exemplar.Emprestimos.Any())
            {
                TempData["Erro"] = "Este exemplar possui histórico de empréstimos e não pode ser excluído. Altere seu status para Inativo.";
                return RedirectToAction(nameof(Index));
            }

            db.Exemplares.Remove(exemplar);
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // CARREGAR LIVROS
    
    private async Task CarregarLivros()
    {
        ViewBag.Livros = await db.Livros
            .OrderBy(x => x.Titulo)
            .ToListAsync();
    }
}