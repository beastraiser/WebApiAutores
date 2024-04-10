using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Controllers.Entidades;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        //---------- GET ---------- api/libros

        [HttpGet]
        public async Task<ActionResult<List<LibroDTO>>> ListadoLibros()
        {
            var libros = await context.Libros.ToListAsync();
            return mapper.Map<List<LibroDTO>>(libros);
        }

        //---------- GET ---------- api/libros/{id:int}

        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros
                /*.Include(libroDB => libroDB.Comentarios)*/ // Incluye el campo Comentarios (que contiene los comentarios de cada libro)
                .Include(libroDB => libroDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Autor)
                .FirstOrDefaultAsync(libroDB => libroDB.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            if (libro == null)
            {
                return NotFound();
            }

            return mapper.Map<LibroDTOConAutores>(libro); // retorna libro como tipo de dato LibroDTO
        }

        //---------- POST ----------

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            // Esta linea verifica que los id introducidos sean contenidos en el campo AutoresIds (Es un JOIN)
            var autoresIds = await context.Autores 
                .Where(autorDB => libroCreacionDTO.AutoresIds.Contains(autorDB.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe alguno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);

            if (libro.AutoresLibros != null)
            {
                for ( var i = 0; i < libro.AutoresLibros.Count; i++ )
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }

            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        }

        //---------- PUT ----------

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound();
            }

            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            // Lleva las propiedades de libroCreacionDTO hacia libroDB, actualizando libroDB
            // Al asignar esta operación a la variable libroDB -> se mantiene la misma instancia que la que se creó más arriba

            AsignarOrdenAutores(libroDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

        //---------- PUT ----------

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null) // Error con el formate que ha enviado el cliente
            {
                return BadRequest();
            }

            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if ( libroDB == null) // El libro con el id proporcionado no existe en la DB
            {
                return NotFound();
            }

            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB); 
            // Se llena le LibroPatchDTO con la info del objeto libroDB
            // Aquí el mapeo va desde (libroDB) hacia <LibroPatchDTO>

            patchDocument.ApplyTo(libroDTO, ModelState); 
            // Se le aplican a libroDTO los cambios que venían en el patchDocument
            // Los errores se guardan en ModelState

            var esValido = TryValidateModel(libroDTO);

            if (!esValido)
            {
                return BadRequest(ModelState); // Aquí se encuentran los errores de validación encontrados
            }

            mapper.Map(libroDTO, libroDB); // Aquí el mapeo va desde libroDTO(que contiene un dato de tipo LibroPatchDTO) hacia Libro

            await context.SaveChangesAsync();
            return NoContent();
        }

        //---------- DELETE ----------

        [HttpDelete("{id:int}")] 
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Libro { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
