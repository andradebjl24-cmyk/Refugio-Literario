using System.Security.Claims;
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

public class LivroController : Controller
{
    private readonly BibliotecaContext db;

    public LivroController(BibliotecaContext db)
    {
        this.db = db;
    }

    // CATÁLOGO COMPLETO DE LIVROS
    public async Task<IActionResult> Index(
        string? termo,
        int? categoriaId,
        int? subcategoriaId)
    {
        var categoriasPrincipais = await db.Categorias
            .AsNoTracking()
            .Where(c => c.CategoriaPaiId == null)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var subcategorias = await db.Categorias
            .AsNoTracking()
            .Where(c => c.CategoriaPaiId != null)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var consulta = db.Livros
            .AsNoTracking()
            .Include(l => l.Categoria)
            .Include(l => l.Subcategorias)
            .Include(l => l.Editora)
            .Include(l => l.Exemplares)
            .Include(l => l.LivrosAutores)
                .ThenInclude(la => la.Autor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            termo = termo.Trim();

            consulta = consulta.Where(l =>
                l.Titulo.Contains(termo) ||
                l.LivrosAutores.Any(la =>
                    la.Autor != null &&
                    la.Autor.Nome.Contains(termo)));
        }

        if (categoriaId.HasValue)
        {
            consulta = consulta.Where(l =>
                l.CategoriaId == categoriaId.Value);
        }

        if (subcategoriaId.HasValue)
        {
            consulta = consulta.Where(l =>
                l.Subcategorias.Any(s =>
                    s.Id == subcategoriaId.Value));
        }

        var livros = await consulta
            .OrderBy(l => l.Titulo)
            .ToListAsync();

        var livrosFavoritos = new HashSet<int>();

        var usuarioId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (int.TryParse(usuarioId, out var idUsuario))
        {
            var idsFavoritos = await db.Favoritos
                .AsNoTracking()
                .Where(f => f.UsuarioId == idUsuario)
                .Select(f => f.LivroId)
                .ToListAsync();

            livrosFavoritos = idsFavoritos.ToHashSet();
        }

        ViewBag.CategoriasPrincipais = categoriasPrincipais;
        ViewBag.Subcategorias = subcategorias;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.SubcategoriaId = subcategoriaId;
        ViewBag.Termo = termo ?? "";
        ViewBag.LivrosFavoritos = livrosFavoritos;

        return View(livros);
    }

    // BUSCAR LIVROS POR TÍTULO OU AUTOR
    public async Task<IActionResult> Buscar(string? termo)
    {
        var consulta = db.Livros
            .AsNoTracking()
            .Include(l => l.Categoria)
            .Include(l => l.Editora)
            .Include(l => l.Exemplares)
            .Include(l => l.LivrosAutores)
                .ThenInclude(la => la.Autor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            termo = termo.Trim();

            consulta = consulta.Where(l =>
                l.Titulo.Contains(termo) ||
                l.LivrosAutores.Any(la =>
                    la.Autor != null &&
                    la.Autor.Nome.Contains(termo)
                )
            );
        }

        var livros = await consulta
            .OrderBy(l => l.Titulo)
            .ToListAsync();

        ViewBag.Termo = termo ?? "";

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var livrosFavoritos = new HashSet<int>();

        if (int.TryParse(usuarioId, out var idUsuario))
        {
            var favoritos = await db.Favoritos
                .AsNoTracking()
                .Where(f => f.UsuarioId == idUsuario)
                .Select(f => f.LivroId)
                .ToListAsync();

            livrosFavoritos = favoritos.ToHashSet();
        }

        ViewBag.LivrosFavoritos = livrosFavoritos;

        return View(livros);
    }

    // DETALHES DO LIVRO

    public async Task<IActionResult> Details(int id)
    {
        var livro = await db.Livros
            .Include(livro => livro.Categoria)
            .Include(livro => livro.Editora)
            .Include(livro => livro.Exemplares)
            .Include(livro => livro.LivrosAutores)
                .ThenInclude(livroAutor => livroAutor.Autor)
            .Include(livro => livro.Comentarios)
                .ThenInclude(comentario => comentario.Usuario)
            .FirstOrDefaultAsync(livro => livro.Id == id);

        if (livro == null)
        {
            return NotFound();
        }

        // Verifica se o usuário atual favoritou este livro.
        var usuarioId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        ViewBag.JaFavoritado = false;

        if (int.TryParse(usuarioId, out var idUsuario))
        {
            ViewBag.JaFavoritado = await db.Favoritos
                .AnyAsync(f =>
                    f.UsuarioId == idUsuario &&
                    f.LivroId == livro.Id
                );
        }

        ViewBag.JaSolicitouEmprestimo = false;
        if (int.TryParse(usuarioId, out var usuarioComId))
        {
            ViewBag.JaSolicitouEmprestimo = await db.Emprestimos
                .Include(x => x.Exemplar)
                .AnyAsync(x => x.UsuarioId == usuarioComId
                    && x.Exemplar != null
                    && x.Exemplar.LivroId == livro.Id
                    && (x.Status == StatusEmprestimo.Solicitado || x.Status == StatusEmprestimo.Ativo));
        }

        return View(livro);
    }


    // LIVROS POR CATEGORIA OU SUBCATEGORIA
    public async Task<IActionResult> Categoria(int id)
    {
        var categoria = await db.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (categoria == null)
        {
            return NotFound();
        }

        var consulta = db.Livros
            .AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.Subcategorias)
            .Include(x => x.Editora)
            .Include(x => x.Exemplares)
            .Include(x => x.LivrosAutores)
                .ThenInclude(la => la.Autor)
            .AsQueryable();

        if (categoria.CategoriaPaiId == null)
        {
            // Categoria principal: livros que pertencem a ela.
            consulta = consulta.Where(x => x.CategoriaId == id);
        }
        else
        {
            // Subcategoria: livros associados especificamente a ela.
            consulta = consulta.Where(x =>
                x.Subcategorias.Any(s => s.Id == id));
        }

        var livros = await consulta
            .OrderBy(x => x.Titulo)
            .ToListAsync();

        ViewBag.NomeCategoria = categoria.Nome;

        return View(livros);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Favoritar(int livroId)
    {
        var usuarioId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(usuarioId, out var usuarioIdInt))
        {
            return RedirectToAction("Login", "Account");
        }

        var usuarioAtivo = await db.Usuarios
            .AnyAsync(x => x.Id == usuarioIdInt && x.Ativo);
        if (!usuarioAtivo)
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        var livroExiste = await db.Livros
            .AnyAsync(x => x.Id == livroId);

        if (!livroExiste)
        {
            return NotFound();
        }

        var favorito = await db.Favoritos
            .FirstOrDefaultAsync(f =>
                f.UsuarioId == usuarioIdInt &&
                f.LivroId == livroId
            );

        if (favorito == null)
        {
            db.Favoritos.Add(new Favorito
            {
                UsuarioId = usuarioIdInt,
                LivroId = livroId
            });
        }
        else
        {
            db.Favoritos.Remove(favorito);
        }

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = livroId });
    }

    // Solicitação de empréstimo: fica pendente até a aprovação do administrador.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> SolicitarEmprestimo(int livroId)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Challenge();

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioId && x.Ativo);

        if (usuario == null)
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        var livroExiste = await db.Livros.AnyAsync(x => x.Id == livroId);
        if (!livroExiste)
            return NotFound();

        // A transação serializável reduz a possibilidade de duas solicitações
        // simultâneas passarem pela mesma verificação antes de gravar.
        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            // A regra considera o LIVRO, não apenas o exemplar escolhido.
            // Assim, outra cópia do mesmo título não permite duplicar o pedido.
            var jaSolicitouOuEmprestado = await db.Emprestimos
                .AnyAsync(x => x.UsuarioId == usuarioId
                    && x.Exemplar != null
                    && x.Exemplar.LivroId == livroId
                    && (x.Status == StatusEmprestimo.Solicitado
                        || x.Status == StatusEmprestimo.Ativo));

            if (jaSolicitouOuEmprestado)
            {
                await transaction.RollbackAsync();
                TempData["ErroEmprestimo"] =
                    "Você já possui uma solicitação pendente ou um empréstimo ativo deste livro.";
                return RedirectToAction(nameof(Details), new { id = livroId });
            }

            var exemplar = await db.Exemplares
                .Where(x => x.LivroId == livroId
                    && x.Status == StatusExemplar.Disponivel)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (exemplar == null)
            {
                await transaction.RollbackAsync();
                TempData["ErroEmprestimo"] =
                    "Não há exemplares disponíveis para solicitar no momento.";
                return RedirectToAction(nameof(Details), new { id = livroId });
            }

            db.Emprestimos.Add(new Emprestimo
            {
                UsuarioId = usuarioId,
                ExemplarId = exemplar.Id,
                DataEmprestimo = DateTime.Today,
                DataPrevistaDevolucao = DateTime.Today.AddDays(15),
                Status = StatusEmprestimo.Solicitado
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["MensagemEmprestimo"] =
                "Solicitação enviada! Ela ficará pendente até a análise do administrador.";
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            TempData["ErroEmprestimo"] =
                "Não foi possível enviar a solicitação. Verifique se você já possui um pedido deste livro e tente novamente.";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return RedirectToAction(nameof(Details), new { id = livroId });
    }
}
