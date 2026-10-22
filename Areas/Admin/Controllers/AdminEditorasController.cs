using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class AdminEditorasController : Controller
{
    private readonly BibliotecaContext db;

    public AdminEditorasController(BibliotecaContext db) => this.db = db;

    public async Task<IActionResult> Index() =>
        View(await db.Editoras.OrderBy(x => x.Nome).ToListAsync());

    public IActionResult Create() => View(new Editora());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Editora model)
    {
        model.Nome = model.Nome?.Trim() ?? "";
        if (!ModelState.IsValid) return View(model);

        try
        {
            db.Editoras.Add(model);
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Editora cadastrado(a) com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível salvar. Verifique os dados e se já existe um registro igual.");
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await db.Editoras.FindAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Editora model)
    {
        if (id != model.Id) return NotFound();
        model.Nome = model.Nome?.Trim() ?? "";
        if (!ModelState.IsValid) return View(model);

        var existente = await db.Editoras.FindAsync(id);
        if (existente == null) return NotFound();
        existente.Nome = model.Nome;
        existente.CNPJ = model.CNPJ?.Trim() ?? "";
        existente.Cidade = model.Cidade?.Trim() ?? "";
        existente.Estado = model.Estado?.Trim() ?? "";
        try
        {
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Alterações salvas com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível salvar as alterações. Verifique os dados.");
            return View(model);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await db.Editoras.FindAsync(id);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var model = await db.Editoras.FindAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var model = await db.Editoras.FindAsync(id);
        if (model == null) return RedirectToAction(nameof(Index));

        try
        {
            db.Editoras.Remove(model);
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Registro excluído com sucesso.";
        }
        catch (DbUpdateException)
        {
            TempData["Erro"] = "Este registro está sendo utilizado por outros dados e não pode ser excluído.";
        }

        return RedirectToAction(nameof(Index));
    }
}
