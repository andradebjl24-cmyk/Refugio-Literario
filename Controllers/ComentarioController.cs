using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace Biblioteca.Controllers;

[Authorize]
public class ComentarioController : Controller
{
    private readonly BibliotecaContext db;
    public ComentarioController(BibliotecaContext db)
    {
        this.db = db;
    }

    // ADICIONAR COMENTÁRIO
   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        int livroId,
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Digite um comentário.")]
        [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "O comentário deve ter até 500 caracteres.")]
        string texto)
    {
        // 1. VERIFICA SE O COMENTÁRIO FOI PREENCHIDO
        
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(texto))
        {
            TempData["ErroComentario"] =
                "Digite um comentário.";

            return RedirectToAction(
                "Details",
                "Livro",
                new { id = livroId });
        }

        // 2. VERIFICA SE O LIVRO EXISTE
        
        var livroExiste = await db.Livros
            .AnyAsync(x => x.Id == livroId);

        if (!livroExiste)
        {
            return NotFound();
        }

        // 3. PEGA O ID DO USUÁRIO LOGADO
        
        var usuarioIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (usuarioIdClaim == null)
        {
            return Unauthorized();
        }

        // 4. CONVERTE O ID DO USUÁRIO
        
        if (!int.TryParse(
                usuarioIdClaim.Value,
                out int usuarioId))
        {
            return Unauthorized();
        }

        // 5. VERIFICA SE O USUÁRIO EXISTE
        
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioId);

        if (usuario == null || !usuario.Ativo)
        {
            return Unauthorized();
        }

        // 6. CRIA O COMENTÁRIO
        
        var comentario = new Comentario
        {
            Texto = texto.Trim(),
            Data = DateTime.Now,
            UsuarioId = usuario.Id,
            LivroId = livroId
        };

        // 7. SALVA NO BANCO DE DADOS
       
        db.Comentarios.Add(comentario);

        await db.SaveChangesAsync();

        // 8. VOLTA PARA OS DETALHES DO LIVRO
       
        return RedirectToAction(
            "Details",
            "Livro",
            new { id = livroId });
    }
}
