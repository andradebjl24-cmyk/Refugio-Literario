
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class AdminLivrosController : Controller
{
    private readonly BibliotecaContext db;
    private readonly IWebHostEnvironment environment;

    public AdminLivrosController(
        BibliotecaContext db,
        IWebHostEnvironment environment)
    {
        this.db = db;
        this.environment = environment;
    }

    // LISTAGEM

    public async Task<IActionResult> Index()
    {
        var livros = await db.Livros
            .Include(x => x.Categoria)
            .Include(x => x.Editora)
            .Include(x => x.Exemplares)
            .OrderBy(x => x.Titulo)
            .ToListAsync();

        return View(livros);
    }

    // CRIAR - GET

    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasEEditoras();

        return View(new Livro());
    }

    // CRIAR - POST

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Livro m,
        IFormFile? imagem,
        int[]? autoresSelecionados,
        int[]? subcategoriasSelecionadas)
    {
        var categoriaValida = await db.Categorias.AnyAsync(c =>
            c.Id == m.CategoriaId &&
            c.CategoriaPaiId == null);

        if (!categoriaValida)
        {
            ModelState.AddModelError(
                nameof(Livro.CategoriaId),
                "Selecione uma categoria principal válida.");
        }

        var idsSubcategorias = (subcategoriasSelecionadas
            ?? Array.Empty<int>())
            .Distinct()
            .ToArray();

        var subcategoriasValidas = idsSubcategorias.Length == 0
            || await db.Categorias.CountAsync(c =>
                idsSubcategorias.Contains(c.Id) &&
                c.CategoriaPaiId == m.CategoriaId)
                == idsSubcategorias.Length;

        if (!subcategoriasValidas)
        {
            ModelState.AddModelError(
                "Subcategorias",
                "Selecione somente subcategorias da categoria principal escolhida.");
        }

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasEEditoras(
                autoresSelecionados,
                idsSubcategorias,
                m.CategoriaId);

            return View(m);
        }

        try
        {
            if (imagem != null && imagem.Length > 0)
                m.Imagem = await SalvarImagem(imagem);

            var subcategorias = await db.Categorias
                .Where(c => idsSubcategorias.Contains(c.Id))
                .ToListAsync();

            await using var transacao =
                await db.Database.BeginTransactionAsync();

            m.Subcategorias = subcategorias;

            db.Livros.Add(m);
            await db.SaveChangesAsync();

            if (autoresSelecionados != null)
            {
                foreach (var autorId in autoresSelecionados.Distinct())
                {
                    if (await db.Autores.AnyAsync(a => a.Id == autorId))
                    {
                        db.LivrosAutores.Add(new LivroAutor
                        {
                            LivroId = m.Id,
                            AutorId = autorId
                        });
                    }
                }

                await db.SaveChangesAsync();
            }

            await transacao.CommitAsync();

            TempData["Mensagem"] = "Livro cadastrado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(imagem), ex.Message);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                nameof(Livro.ISBN),
                "Não foi possível salvar. Verifique se o ISBN já está cadastrado e se a categoria e a editora existem.");
        }

        await CarregarCategoriasEEditoras(
            autoresSelecionados,
            idsSubcategorias,
            m.CategoriaId);

        return View(m);
    }

    // EDITAR - GET

    public async Task<IActionResult> Edit(int id)
    {
        var livro = await db.Livros
            .Include(x => x.Subcategorias)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (livro == null)
            return NotFound();

        var autoresSelecionados = await db.LivrosAutores
            .Where(x => x.LivroId == id)
            .Select(x => x.AutorId)
            .ToArrayAsync();

        var subcategoriasSelecionadas = livro.Subcategorias
            .Select(x => x.Id)
            .ToArray();

        await CarregarCategoriasEEditoras(
            autoresSelecionados,
            subcategoriasSelecionadas,
            livro.CategoriaId);

        return View(livro);
    }

    // EDITAR - POST

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Livro m,
        IFormFile? imagem,
        int[]? autoresSelecionados,
        int[]? subcategoriasSelecionadas)
    {
        if (id != m.Id)
            return NotFound();

        var livro = await db.Livros
            .Include(x => x.Subcategorias)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (livro == null)
            return NotFound();

        var categoriaValida = await db.Categorias.AnyAsync(c =>
            c.Id == m.CategoriaId &&
            c.CategoriaPaiId == null);

        if (!categoriaValida)
        {
            ModelState.AddModelError(
                nameof(Livro.CategoriaId),
                "Selecione uma categoria principal válida.");
        }

        var idsSubcategorias = (subcategoriasSelecionadas
            ?? Array.Empty<int>())
            .Distinct()
            .ToArray();

        var subcategoriasValidas = idsSubcategorias.Length == 0
            || await db.Categorias.CountAsync(c =>
                idsSubcategorias.Contains(c.Id) &&
                c.CategoriaPaiId == m.CategoriaId)
                == idsSubcategorias.Length;

        if (!subcategoriasValidas)
        {
            ModelState.AddModelError(
                "Subcategorias",
                "Selecione somente subcategorias da categoria principal escolhida.");
        }

        if (!ModelState.IsValid)
        {
            m.Imagem = livro.Imagem;

            await CarregarCategoriasEEditoras(
                autoresSelecionados,
                idsSubcategorias,
                m.CategoriaId);

            return View(m);
        }

        try
        {
            livro.Titulo = m.Titulo;
            livro.ISBN = m.ISBN;
            livro.AnoPublicacao = m.AnoPublicacao;
            livro.CategoriaId = m.CategoriaId;
            livro.EditoraId = m.EditoraId;
            livro.Destaque = m.Destaque;
            livro.Sinopse = m.Sinopse;

            // Mantém a capa atual se nenhuma nova for enviada.
            if (imagem != null && imagem.Length > 0)
                livro.Imagem = await SalvarImagem(imagem);

            // Atualiza as subcategorias.
            var novasSubcategorias = await db.Categorias
                .Where(c => idsSubcategorias.Contains(c.Id))
                .ToListAsync();

            livro.Subcategorias.Clear();

            foreach (var subcategoria in novasSubcategorias)
                livro.Subcategorias.Add(subcategoria);

            // Atualiza os autores sem duplicar associações.
            var associacoesAtuais = await db.LivrosAutores
                .Where(x => x.LivroId == id)
                .ToListAsync();

            db.LivrosAutores.RemoveRange(associacoesAtuais);

            if (autoresSelecionados != null)
            {
                foreach (var autorId in autoresSelecionados.Distinct())
                {
                    if (await db.Autores.AnyAsync(a => a.Id == autorId))
                    {
                        db.LivrosAutores.Add(new LivroAutor
                        {
                            LivroId = id,
                            AutorId = autorId
                        });
                    }
                }
            }

            await db.SaveChangesAsync();

            TempData["Mensagem"] = "Livro atualizado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(imagem), ex.Message);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                nameof(Livro.ISBN),
                "Não foi possível salvar. Verifique se o ISBN já está cadastrado e se a categoria e a editora existem.");
        }

        m.Imagem = livro.Imagem;

        await CarregarCategoriasEEditoras(
            autoresSelecionados,
            idsSubcategorias,
            m.CategoriaId);

        return View(m);
    }

    // DETALHES

    public async Task<IActionResult> Details(int id)
    {
        var livro = await db.Livros
            .Include(x => x.Categoria)
            .Include(x => x.Editora)
            .Include(x => x.Exemplares)
            .Include(x => x.LivrosAutores)
                .ThenInclude(x => x.Autor)
            .Include(x => x.Subcategorias)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (livro == null)
            return NotFound();

        return View(livro);
    }

    // EXCLUIR - GET

    public async Task<IActionResult> Delete(int id)
    {
        var livro = await db.Livros
            .FirstOrDefaultAsync(x => x.Id == id);

        if (livro == null)
            return NotFound();

        return View(livro);
    }

    // EXCLUIR - POST

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteOk(int id)
    {
        var livro = await db.Livros.FindAsync(id);

        if (livro != null)
        {
            try
            {
                db.Livros.Remove(livro);
                await db.SaveChangesAsync();

                TempData["Mensagem"] = "Livro excluído com sucesso.";
            }
            catch (DbUpdateException)
            {
                TempData["Erro"] =
                    "Este livro possui exemplares ou histórico relacionado. Remova ou inative os registros relacionados antes de excluí-lo.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // SALVAR IMAGEM

    private async Task<string> SalvarImagem(IFormFile imagem)
    {
        var extensoesPermitidas = new[]
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        var extensao = Path.GetExtension(imagem.FileName)
            .ToLowerInvariant();

        if (!extensoesPermitidas.Contains(extensao))
            throw new InvalidOperationException(
                "Formato de imagem não permitido. Use JPG, PNG ou WEBP.");

        if (imagem.Length <= 0)
            throw new InvalidOperationException(
                "O arquivo enviado está vazio.");

        if (imagem.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException(
                "A imagem deve ter no máximo 5 MB.");

        var cabecalho = new byte[12];

        await using (var leitura = imagem.OpenReadStream())
        {
            var lidos = await leitura.ReadAsync(
                cabecalho.AsMemory(0, cabecalho.Length));

            var png = lidos >= 8 &&
                cabecalho.Take(8).SequenceEqual(new byte[]
                {
                    137, 80, 78, 71, 13, 10, 26, 10
                });

            var jpeg = lidos >= 3 &&
                cabecalho[0] == 0xFF &&
                cabecalho[1] == 0xD8 &&
                cabecalho[2] == 0xFF;

            var webp = lidos >= 12 &&
                cabecalho[0] == (byte)'R' &&
                cabecalho[1] == (byte)'I' &&
                cabecalho[2] == (byte)'F' &&
                cabecalho[3] == (byte)'F' &&
                cabecalho[8] == (byte)'W' &&
                cabecalho[9] == (byte)'E' &&
                cabecalho[10] == (byte)'B' &&
                cabecalho[11] == (byte)'P';

            var assinaturaValida = extensao switch
            {
                ".png" => png,
                ".jpg" or ".jpeg" => jpeg,
                ".webp" => webp,
                _ => false
            };

            if (!assinaturaValida)
                throw new InvalidOperationException(
                    "O conteúdo do arquivo não corresponde a uma imagem JPG, PNG ou WEBP válida.");
        }

        if (string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            throw new InvalidOperationException(
                "O caminho da pasta wwwroot não foi encontrado.");
        }

        var pasta = Path.Combine(
            environment.WebRootPath,
            "images",
            "livros");

        Directory.CreateDirectory(pasta);

        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
        var caminho = Path.Combine(pasta, nomeArquivo);

        await using var stream = new FileStream(
            caminho,
            FileMode.Create);

        await imagem.CopyToAsync(stream);

        return $"/images/livros/{nomeArquivo}";
    }

    // CARREGAR LISTAS PARA OS FORMULÁRIOS

    private async Task CarregarCategoriasEEditoras(
        int[]? autoresSelecionados = null,
        int[]? subcategoriasSelecionadas = null,
        int? categoriaPrincipalId = null)
    {
        ViewBag.Autores = await db.Autores
            .OrderBy(x => x.Nome)
            .ToListAsync();

        ViewBag.AutoresSelecionados =
            autoresSelecionados ?? Array.Empty<int>();

        ViewBag.CategoriasPrincipais = await db.Categorias
            .Where(x => x.CategoriaPaiId == null)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        ViewBag.Subcategorias = await db.Categorias
            .Where(x => x.CategoriaPaiId != null)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        ViewBag.SubcategoriasSelecionadas =
            subcategoriasSelecionadas ?? Array.Empty<int>();

        ViewBag.CategoriaPrincipalSelecionada = categoriaPrincipalId;

        ViewBag.Editoras = await db.Editoras
            .OrderBy(x => x.Nome)
            .ToListAsync();
    }
}
