using Biblioteca.Data; 
using Biblioteca.ViewModels; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Areas.Admin.Controllers;

[Area("Admin"),Authorize(Roles="Administrador")]
public class AdminController:Controller 
{ readonly BibliotecaContext db; public AdminController(BibliotecaContext db)=>this.db=db;
public async Task<IActionResult> Index()=>View(new DashboardViewModel
{Usuarios=await db.Usuarios.CountAsync()
    ,Livros=await db.Livros.CountAsync()
    ,Exemplares=await db.Exemplares.CountAsync()
    ,Emprestimos=await db.Emprestimos.CountAsync(x=>x.Status==Models.StatusEmprestimo.Ativo)}); }