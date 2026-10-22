using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class AdminCategoriasController : Controller
{
    private readonly BibliotecaContext db;

    public AdminCategoriasController(BibliotecaContext db)
    {
        this.db = db;
    }

    // LISTAR CATEGORIAS
    public async Task<IActionResult> Index()
    {
        var categorias = await db.Categorias
            .Include(c => c.CategoriaPai)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return View(categorias);
    }

    // ABRIR FORMULARIO DE CADASTRO
    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasPaiAsync();

        return View(new Categoria());
    }


    // CADASTRAR CATEGORIA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria m)
    {
        await ValidarCategoriaAsync(m);

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasPaiAsync(
                categoriaPaiSelecionadaId: m.CategoriaPaiId);

            return View(m);
        }

        try
        {
            db.Categorias.Add(m);
            await db.SaveChangesAsync();

            TempData["Mensagem"] = "Categoria cadastrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "",
                "Não foi possível salvar a categoria. Verifique os dados e tente novamente.");

            await CarregarCategoriasPaiAsync(
                categoriaPaiSelecionadaId: m.CategoriaPaiId);

            return View(m);
        }
    }


    // ABRIR FORMULARIO DE EDICAO
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);

        if (categoria == null)
            return NotFound();

        await CarregarCategoriasPaiAsync(
            categoriaAtualId: categoria.Id,
            categoriaPaiSelecionadaId: categoria.CategoriaPaiId);

        return View(categoria);
    }

    // SALVAR EDICAO
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria m)
    {
        if (id != m.Id)
            return NotFound();

        var categoriaExistente = await db.Categorias.FindAsync(id);

        if (categoriaExistente == null)
            return NotFound();

        await ValidarCategoriaAsync(m, categoriaExistente.Id);

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasPaiAsync(
                categoriaAtualId: categoriaExistente.Id,
                categoriaPaiSelecionadaId: m.CategoriaPaiId);

            return View(m);
        }

        // Atualiza apenas os campos editáveis.
        categoriaExistente.Nome = m.Nome;
        categoriaExistente.CategoriaPaiId = m.CategoriaPaiId;
        categoriaExistente.ExibirNaHome = m.ExibirNaHome;

        try
        {
            await db.SaveChangesAsync();

            TempData["Mensagem"] = "Categoria atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "",
                "Não foi possível atualizar a categoria. Verifique os dados e tente novamente.");

            await CarregarCategoriasPaiAsync(
                categoriaAtualId: categoriaExistente.Id,
                categoriaPaiSelecionadaId: m.CategoriaPaiId);

            return View(m);
        }
    }

    // VER DETALHES
    public async Task<IActionResult> Details(int id)
    {
        var categoria = await db.Categorias
            .Include(c => c.CategoriaPai)
            .Include(c => c.Subcategorias)
            .Include(c => c.Livros)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound();

        return View(categoria);
    }

    // ABRIR CONFIRMACAO DE EXCLUSAO
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await db.Categorias
            .Include(c => c.CategoriaPai)
            .Include(c => c.Subcategorias)
            .Include(c => c.Livros)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound();

        return View(categoria);
    }

    // EXCLUIR CATEGORIA
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteOk(int id)
    {
        var categoria = await db.Categorias
            .Include(c => c.Subcategorias)
            .Include(c => c.Livros)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return RedirectToAction(nameof(Index));

        if (categoria.Subcategorias.Any())
        {
            TempData["Erro"] =
                "Não é possível excluir esta categoria porque ela possui subcategorias.";
        }
        else if (categoria.Livros.Any())
        {
            TempData["Erro"] =
                "Não é possível excluir esta categoria porque ela está vinculada a livros.";
        }
        else
        {
            try
            {
                db.Categorias.Remove(categoria);
                await db.SaveChangesAsync();

                TempData["Mensagem"] = "Categoria excluída com sucesso.";
            }
            catch (DbUpdateException)
            {
                TempData["Erro"] =
                    "Não foi possível excluir esta categoria porque existem registros vinculados a ela.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // CARREGAR AS OPCOES DO CAMPO "CATEGORIA PRINCIPAL"
    private async Task CarregarCategoriasPaiAsync(
        int? categoriaAtualId = null,
        int? categoriaPaiSelecionadaId = null)
    {
        var todas = await db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var idsBloqueados = new HashSet<int>();

        if (categoriaAtualId.HasValue)
        {
            // Não permite escolher a própria categoria nem uma descendente.
            idsBloqueados.Add(categoriaAtualId.Value);

            var fila = new Queue<int>();
            fila.Enqueue(categoriaAtualId.Value);

            while (fila.Count > 0)
            {
                var paiId = fila.Dequeue();

                var filhas = todas
                    .Where(c => c.CategoriaPaiId == paiId);

                foreach (var filha in filhas)
                {
                    if (idsBloqueados.Add(filha.Id))
                        fila.Enqueue(filha.Id);
                }
            }
        }

        var opcoes = todas
            .Where(c => !idsBloqueados.Contains(c.Id))
            .ToList();

        ViewBag.CategoriasPai = new SelectList(
            opcoes,
            "Id",
            "Nome",
            categoriaPaiSelecionadaId);
    }

    // VALIDAR O RELACIONAMENTO ENTRE CATEGORIAS
    private async Task ValidarCategoriaAsync(
        Categoria m,
        int? categoriaAtualId = null)
    {
        if (m.CategoriaPaiId.HasValue)
        {
            var paiExiste = await db.Categorias
                .AnyAsync(c => c.Id == m.CategoriaPaiId.Value);

            if (!paiExiste)
            {
                ModelState.AddModelError(
                    nameof(m.CategoriaPaiId),
                    "A categoria principal selecionada não existe.");
            }

            if (categoriaAtualId.HasValue)
            {
                var todas = await db.Categorias
                    .AsNoTracking()
                    .ToListAsync();

                var idsBloqueados = new HashSet<int>
                {
                    categoriaAtualId.Value
                };

                var fila = new Queue<int>();
                fila.Enqueue(categoriaAtualId.Value);

                while (fila.Count > 0)
                {
                    var paiId = fila.Dequeue();

                    foreach (var filha in todas.Where(
                        c => c.CategoriaPaiId == paiId))
                    {
                        if (idsBloqueados.Add(filha.Id))
                            fila.Enqueue(filha.Id);
                    }
                }

                if (idsBloqueados.Contains(m.CategoriaPaiId.Value))
                {
                    ModelState.AddModelError(
                        nameof(m.CategoriaPaiId),
                        "Não é possível selecionar a própria categoria ou uma de suas subcategorias como categoria principal.");
                }
            }

            if (m.ExibirNaHome)
            {
                ModelState.AddModelError(
                    nameof(m.ExibirNaHome),
                    "Somente categorias principais podem aparecer nos destaques da página inicial.");
            }
        }
    }
}
