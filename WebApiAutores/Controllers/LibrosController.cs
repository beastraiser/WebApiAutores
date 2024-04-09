using AutoMapper;
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(libroDB => libroDB.Comentarios) // Incluye el campo Comentarios (que contiene los comentarios de cada libro)
                .FirstOrDefaultAsync(libroDB => libroDB.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            return mapper.Map<LibroDTO>(libro); // retorna libro como tipo de dato LibroDTO
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

            if (libro.AutoresLibros != null)
            {
                for ( var i = 0; i < libro.AutoresLibros.Count; i++ )
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }

            context.Add(libro);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
