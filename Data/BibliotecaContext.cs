using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public class BibliotecaContext : DbContext
{
    public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Editora> Editoras => Set<Editora>();
    public DbSet<Exemplar> Exemplares => Set<Exemplar>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<LivroAutor> LivrosAutores => Set<LivroAutor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LivroAutor>()
            .HasKey(x => new { x.LivroId, x.AutorId });

        modelBuilder.Entity<LivroAutor>()
            .HasOne(x => x.Livro)
            .WithMany(x => x.LivrosAutores)
            .HasForeignKey(x => x.LivroId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LivroAutor>()
            .HasOne(x => x.Autor)
            .WithMany(x => x.LivrosAutores)
            .HasForeignKey(x => x.AutorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.CPF)
            .IsUnique();

        modelBuilder.Entity<Livro>()
            .HasIndex(x => x.ISBN)
            .IsUnique();

        modelBuilder.Entity<Exemplar>()
            .HasIndex(x => new { x.LivroId, x.Tombo })
            .IsUnique();

        modelBuilder.Entity<Favorito>()
            .HasIndex(x => new { x.UsuarioId, x.LivroId })
            .IsUnique();

        // SQL Server: um exemplar pode ter apenas um empréstimo ativo.
        // StatusEmprestimo.Ativo = 1 (ver Models/Enums.cs).
        modelBuilder.Entity<Emprestimo>()
            .HasIndex(x => x.ExemplarId)
            .IsUnique()
            .HasFilter("[Status] = 1");

        modelBuilder.Entity<Emprestimo>()
            .HasOne(x => x.Usuario)
            .WithMany(x => x.Emprestimos)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Emprestimo>()
            .HasOne(x => x.Exemplar)
            .WithMany(x => x.Emprestimos)
            .HasForeignKey(x => x.ExemplarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Livro>()
            .HasOne(x => x.Categoria)
            .WithMany(x => x.Livros)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Livro>()
            .HasMany(x => x.Subcategorias)
            .WithMany(x => x.LivrosComSubcategoria)
            .UsingEntity<Dictionary<string, object>>(
                "LivroSubcategoria",
                relacionamento => relacionamento
                    .HasOne<Categoria>()
                    .WithMany()
                    .HasForeignKey("CategoriaId")
                    .OnDelete(DeleteBehavior.Restrict),
                relacionamento => relacionamento
                    .HasOne<Livro>()
                    .WithMany()
                    .HasForeignKey("LivroId")
                    .OnDelete(DeleteBehavior.Cascade),
                relacionamento =>
                {
                    relacionamento.HasKey("LivroId", "CategoriaId");
                });

        modelBuilder.Entity<Favorito>()
            .HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favorito>()
            .HasOne(x => x.Livro)
            .WithMany()
            .HasForeignKey(x => x.LivroId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comentario>()
            .HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comentario>()
            .HasOne(x => x.Livro)
            .WithMany(x => x.Comentarios)
            .HasForeignKey(x => x.LivroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
