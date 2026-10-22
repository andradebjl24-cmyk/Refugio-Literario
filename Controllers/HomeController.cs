using Biblioteca.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

public class HomeController : Controller
{
    private readonly BibliotecaContext db;

    public HomeController(BibliotecaContext db)
    {
        this.db = db;
    }

    // PÁGINA INICIAL
    public async Task<IActionResult> Index()
    {
        // LIVROS EM DESTAQUE
        // Mostra os livros marcados pelo administrador como destaque.
        var destaques = await db.Livros
            .Include(x => x.Categoria)
            .Where(x => x.Destaque)
            .OrderBy(x => x.Titulo)
            .Take(4)
            .ToListAsync();

        // LIVROS RECENTES
        // Mostra os últimos livros cadastrados no acervo.
        var recentes = await db.Livros
            .Include(x => x.Categoria)
            .OrderByDescending(x => x.Id)
            .Take(5)
            .ToListAsync();


        // CATEGORIAS
        // Mostra somente as categorias principais
        // selecionadas para aparecer na página inicial.
        var categorias = await db.Categorias
            .Where(x => x.ExibirNaHome && x.CategoriaPaiId == null)
            .OrderBy(x => x.Nome)
            .ToListAsync();


        //COMENTÁRIOS 
        //carrega comentarios mais recentes junto do usuario e livro 

        var comentarios = await db.Comentarios
            .Include(x => x.Usuario)
            .Include(x => x.Livro)
            .OrderByDescending(x => x.Data)
            .Take(3)
            .ToListAsync();

        // DADOS DA HOME
        ViewBag.Recentes = recentes;
        ViewBag.Categorias = categorias;
        ViewBag.Comentarios = comentarios;

        // Os livros em destaque são o Model da View.
        return View(destaques);
    }
}