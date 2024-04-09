namespace WebApiAutores.Controllers.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Contenido { get; set; }
        public int LibroId { get; set; }

        // Propiedades de navegación -> permite cargar datos de otras tablas si así se desea
        public Libro Libro { get; set; }
    }
}
