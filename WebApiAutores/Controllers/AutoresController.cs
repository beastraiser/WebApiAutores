using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApiAutores.Controllers.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")] //Esta es la ruta a través de la cual se accede a este controlador. Se podría poner solo /autores pero por buenas prácticas se pone api/autores, para saber que se está accediendo a una api
    public class AutoresController: ControllerBase //Esto es el controlador => clase que define la tabla y contiene los métodos para gestionar esa tabla (endpoints)
    {

        private readonly ApplicationDbContext context;

        public AutoresController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet] //Si se ejecuta una petición GET hacia este controlador, se ejecutará el código que hay dentro de este bloque
        public async Task<ActionResult<List<Autor>>> Get() //Este endpoint devuelve la lista con todos los datos de la tabla Autor. La ruta es "api/autores"
        {
            return await context.Autores.Include(x => x.Libros).ToListAsync();
        }

        [HttpGet("primero")] //Este es otro endpoint GET cuya ruta va a ser "api/autores/primero". Si no se indica ninguna cadena dará error ya que habrá dos endpoints GET para la misma ruta
        public async Task<ActionResult<Autor>> PrimerAutor() //Este endpoint devuelve el primer autor de la tabla Autor o un null si no hay ningún valor
        {
            return await context.Autores.FirstOrDefaultAsync();
        }

        [HttpPost] //Si se ejecuta una petición POST hacia este controlador, se ejecutará el código que hay dentro de este bloque
        public async Task<ActionResult> Post(Autor autor)
        {
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")] //api/autores/1
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            if (autor.Id !=id)
            {
                return BadRequest("El id del autor no coincide con el id de la URL");
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")] //api/autores/2
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x=> x.Id ==id);
            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor { Id = id});
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
