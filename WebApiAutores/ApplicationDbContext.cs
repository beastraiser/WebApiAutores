using Microsoft.EntityFrameworkCore;
using WebApiAutores.Controllers.Entidades;

namespace WebApiAutores
{
    // Clase central de Entity Framework Core (EF Core), a través de la cual se configuran las tablas de la DB
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        // Code First -> a partir de sentencias C# creamos una DB
        // DB First -> a partir de una DB generamos las sentencias C#

        //DBSet -> crea un tabla a partir de la clase <clase>
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
    }
}
