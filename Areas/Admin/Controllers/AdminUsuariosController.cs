using Biblioteca.Data;
using Biblioteca.Models;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class AdminUsuariosController : Controller
{
    private readonly BibliotecaContext db;
    private readonly PasswordHasher<Usuario> hasher = new();

    public AdminUsuariosController(BibliotecaContext db) => this.db = db;

    public async Task<IActionResult> Index() =>
        View(await db.Usuarios.OrderBy(x => x.Nome).ToListAsync());

    public IActionResult Create() => View(new AdminUsuarioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUsuarioViewModel model)
    {
        ValidarSenha(model.Senha, obrigatoria: true);
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var cpf = new string(model.CPF.Where(char.IsDigit).ToArray());
        if (cpf.Length != 11)
            ModelState.AddModelError(nameof(model.CPF), "Informe um CPF com 11 dígitos.");
        if (!ModelState.IsValid) return View(model);

        if (await db.Usuarios.AnyAsync(x => x.Email == email))
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
        if (await db.Usuarios.AnyAsync(x => x.CPF == cpf))
            ModelState.AddModelError(nameof(model.CPF), "Este CPF já está cadastrado.");
        if (!ModelState.IsValid) return View(model);

        var usuario = new Usuario
        {
            Nome = model.Nome.Trim(),
            CPF = cpf,
            Email = email,
            TipoUsuario = model.TipoUsuario,
            Ativo = model.Ativo
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha!);

        try
        {
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Usuário cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível cadastrar. O e-mail ou CPF pode já estar em uso.");
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        return View(new AdminUsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            CPF = usuario.CPF,
            Email = usuario.Email,
            TipoUsuario = usuario.TipoUsuario,
            Ativo = usuario.Ativo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminUsuarioViewModel model)
    {
        if (id != model.Id) return NotFound();
        ValidarSenha(model.Senha, obrigatoria: false);
        if (!ModelState.IsValid) return View(model);

        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        var email = model.Email.Trim();
        var cpf = new string(model.CPF.Where(char.IsDigit).ToArray());
        if (cpf.Length != 11)
            ModelState.AddModelError(nameof(model.CPF), "Informe um CPF com 11 dígitos.");
        if (await db.Usuarios.AnyAsync(x => x.Id != id && x.Email == email))
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já pertence a outro usuário.");
        if (await db.Usuarios.AnyAsync(x => x.Id != id && x.CPF == cpf))
            ModelState.AddModelError(nameof(model.CPF), "Este CPF já pertence a outro usuário.");
        if (!ModelState.IsValid) return View(model);

        usuario.Nome = model.Nome.Trim();
        usuario.CPF = cpf;
        usuario.Email = email;
        usuario.TipoUsuario = model.TipoUsuario;
        usuario.Ativo = model.Ativo;
        if (!string.IsNullOrWhiteSpace(model.Senha))
            usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha);

        try
        {
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Usuário atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível salvar. Verifique se o e-mail e o CPF são únicos.");
            return View(model);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        return usuario == null ? NotFound() : View(usuario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        return usuario == null ? NotFound() : View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await db.Usuarios
            .Include(x => x.Emprestimos)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (usuario == null) return RedirectToAction(nameof(Index));

        if (User.Identity?.Name == usuario.Nome &&
            usuario.TipoUsuario == TipoUsuario.Administrador)
        {
            TempData["Erro"] = "Não é possível excluir a própria conta administrativa durante a sessão.";
            return RedirectToAction(nameof(Index));
        }

        if (usuario.Emprestimos.Any())
        {
            TempData["Erro"] = "Este usuário possui histórico de empréstimos. Desative a conta em vez de excluí-la.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            db.Usuarios.Remove(usuario);
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Usuário excluído com sucesso.";
        }
        catch (DbUpdateException)
        {
            TempData["Erro"] = "Não foi possível excluir o usuário porque existem registros relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidarSenha(string? senha, bool obrigatoria)
    {
        if (obrigatoria && string.IsNullOrWhiteSpace(senha))
            ModelState.AddModelError(nameof(AdminUsuarioViewModel.Senha), "Informe uma senha.");
        else if (!string.IsNullOrWhiteSpace(senha) && senha.Length < 8)
            ModelState.AddModelError(nameof(AdminUsuarioViewModel.Senha), "A senha deve ter ao menos 8 caracteres.");
    }
}
