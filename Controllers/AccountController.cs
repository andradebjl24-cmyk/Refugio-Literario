using Biblioteca.Data;
using Biblioteca.Models;
using Biblioteca.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace Biblioteca.Controllers;

public class AccountController : Controller
{
    private readonly BibliotecaContext db;
    private readonly PasswordHasher<Usuario> hash = new();

    public AccountController(BibliotecaContext db)
    {
        this.db = db;
    }

    // LOGIN
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel m,
        string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(m);
        }

        var emailNormalizado = m.Email.Trim();

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(x =>
                x.Email == emailNormalizado &&
                x.Ativo);

        if (usuario == null ||
            hash.VerifyHashedPassword(
                usuario,
                usuario.SenhaHash,
                m.Senha) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(
                string.Empty,
                "E-mail ou senha inválidos.");

            return View(m);
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                usuario.Nome),

            new Claim(
                ClaimTypes.Email,
                usuario.Email),

            new Claim(
                ClaimTypes.Role,
                usuario.TipoUsuario.ToString())
        };

        var identidade = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var usuarioLogado = new ClaimsPrincipal(identidade);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            usuarioLogado,
            new AuthenticationProperties
            {
                IsPersistent = m.Lembrar,
                AllowRefresh = true
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (usuario.TipoUsuario == TipoUsuario.Administrador)
        {
            return RedirectToAction(
                "Index",
                "Admin",
                new { area = "Admin" });
        }

        return RedirectToAction("Index", "Home");
    }

    // CADASTRO
    
    [HttpGet]
    public IActionResult Cadastro()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(CadastroViewModel m)
    {
        // Valida antes de acessar os campos.
        if (!ModelState.IsValid)
        {
            return View(m);
        }

        var emailNormalizado = m.Email.Trim();

        // Verifica se o e-mail já está cadastrado.
        var emailExiste = await db.Usuarios
            .AnyAsync(x => x.Email == emailNormalizado);

        if (emailExiste)
        {
            ModelState.AddModelError(
                nameof(m.Email),
                "Este e-mail já está cadastrado.");

            return View(m);
        }

        // Normaliza e valida o CPF.
        var cpfNormalizado = new string(
            m.CPF.Where(char.IsDigit).ToArray());

        if (cpfNormalizado.Length != 11)
        {
            ModelState.AddModelError(
                nameof(m.CPF),
                "Informe um CPF com 11 dígitos.");

            return View(m);
        }

        // Verifica se o CPF já está cadastrado.
        var cpfExiste = await db.Usuarios
            .AnyAsync(x => x.CPF == cpfNormalizado);

        if (cpfExiste)
        {
            ModelState.AddModelError(
                nameof(m.CPF),
                "Este CPF já está cadastrado.");

            return View(m);
        }

        // Cria o usuário comum.
        var usuario = new Usuario
        {
            Nome = m.Nome.Trim(),
            CPF = cpfNormalizado,
            Email = emailNormalizado,
            TipoUsuario = TipoUsuario.UsuarioComum,
            Ativo = true
        };

        // Armazena somente o hash da senha.
        usuario.SenhaHash = hash.HashPassword(
            usuario,
            m.Senha);

        db.Usuarios.Add(usuario);

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    // PERFIL - VISUALIZAR
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var usuarioId))
        {
            return RedirectToAction(nameof(Login));
        }

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioId);

        if (usuario == null)
        {
            return NotFound();
        }

        var model = new PerfilViewModel
        {
            Nome = usuario.Nome,
            CPF = usuario.CPF,
            Email = usuario.Email,
            Avatar = usuario.Avatar
        };

        return View(model);
    }

    // PERFIL - EDITAR
    
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(PerfilViewModel m)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var usuarioId))
        {
            return RedirectToAction(nameof(Login));
        }

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioId);

        if (usuario == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            // CPF e e-mail são somente leitura neste formulário.
            m.CPF = usuario.CPF;
            m.Email = usuario.Email;

            return View(m);
        }

        // Atualiza somente os campos editáveis.
        usuario.Nome = m.Nome.Trim();
        usuario.Avatar = m.Avatar;

        await db.SaveChangesAsync();

        // Atualiza as informações da sessão autenticada.
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                usuario.Nome),

            new Claim(
                ClaimTypes.Email,
                usuario.Email),

            new Claim(
                ClaimTypes.Role,
                usuario.TipoUsuario.ToString())
        };

        var identidade = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade));

        return RedirectToAction(nameof(Perfil));
    }

    // FAVORITOS
   
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Favoritos()
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var usuarioId))
        {
            return RedirectToAction(nameof(Login));
        }

        var favoritos = await db.Favoritos
            .Include(x => x.Livro)
                .ThenInclude(x => x.Categoria)
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return View(favoritos);
    }

    // LOGOUT
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    // ACESSO NEGADO
   
    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }

    // MEUS EMPRÉSTIMOS
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Emprestimos()
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var usuarioId))
        {
            return RedirectToAction(nameof(Login));
        }

        var emprestimos = await db.Emprestimos
            .Include(x => x.Exemplar)
                .ThenInclude(x => x.Livro)
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.DataEmprestimo)
            .ToListAsync();

        return View(emprestimos);
    }
}
