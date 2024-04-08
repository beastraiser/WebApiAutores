using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApiAutores.Controllers.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    // |-----------------------------|
    // |  DEFINICION DE CONTROLADOR  | 
    // |-----------------------------|

    [ApiController] // Retorna un error 4XX si hay un error a nivel de modelo
    [Route("api/autores")]
    //Esta es la ruta a través de la cual se accede a este controlador.
    //Se puede poner un placeholder entre corchetes "api/[controller]", el cual indicará el nombre del controlador "[autores]/controller", en este caso "autores".
    //Se podría poner solo /autores pero por buenas prácticas se pone api/autores, para saber que se está accediendo a una api.
    //[Authorize] // Filtro de tubería a nivel de controlador -> no permite acceder a ninguna ruta a no ser que sea a través de un usuario autorizado
    public class AutoresController: ControllerBase //Esto es el controlador => clase que define la tabla y contiene los métodos para gestionar esa tabla (endpoints)
    {

        private readonly ApplicationDbContext context;
        private readonly IServicio servicio;
        private readonly ServicioTransient servicioTransient;
        private readonly ServicioScoped servicioScoped;
        private readonly ServicioSingleton servicioSingleton;
        private readonly ILogger<AutoresController> logger;

        public AutoresController(ApplicationDbContext context, IServicio servicio, ServicioTransient servicioTransient, ServicioScoped servicioScoped, ServicioSingleton servicioSingleton, ILogger<AutoresController> logger) 
            // IServicio -> Esto se llama inyeccion de dependencias.
            // Se inyecta la interfaz que contiene múltiples servicios, en vez de un servicio concreto.
            // Esto se llama PRINCIPIO SOLID -> Depender de abstracciones y no de tipos concretos.
            // Todas las interfaces se encuentran en la carpeta Servicios.
        {
            this.context = context;
            this.servicio = servicio;
            this.servicioTransient = servicioTransient;
            this.servicioScoped = servicioScoped;
            this.servicioSingleton = servicioSingleton;
            this.logger = logger;
        }

        // |-----------------------------|
        // | DEFINICION DE LOS ENDPOINTS | 
        // |-----------------------------|

        // La asincronía es necesaria cada vez que se pide cualquier recurso externo a nuestra aplicación.
        // Estos recursos pueden ser de una API, una Base de Datos o un Sistema Operativo.
        // Para aplicarla a los endpoints se utiliza:
        // - el tipo "async" y el tipo de dato "Task<>" en la definición de la función.
        // - el tipo "await" para los recursos a recibir.


        //---------- GET ---------- api/autores/sincrono

        [HttpGet("sincrono")] // -> Atributo del endpoint. Concatena su contenido a la ruta heredada del controlador.
                              // Si no hubiese ningún atributo heredaría la ruta del controlador: "api/autores".
                              // Si se ejecuta una petición GET hacia este controlador, se ejecutará el código que hay dentro de este bloque.
        public List<Autor> ResultadoSincrono()
        {
            return context.Autores.Include(x => x.Libros).ToList();
            // Es un ejemplo de endpoint sin asincronía. Returna un dato de tipo Lista de Autor.
            // Esto funciona perfectamente pero no se podría retornar ningun dato que no fuese de Autor, como NotFound() para el manejo de errores.

        }

        //---------- GET ---------- api/autores/asincrono

        [HttpGet("asincrono")]
        public async Task<List<Autor>> ResultadoAsincrono() // Se añade "async" y "Task<>".
        {
            return await context.Autores.Include(x => x.Libros).ToListAsync(); // Se añade "await".
            // Es el mismo endpoint de arriba pero habiendo aplicado la asincronía.
            // El método ToList() no puede devolver datos de tipo Task<> -> cambia a ToListAsync().

        }

        //---------- GET ---------- api/autores/actionresult/{id}

        [HttpGet("actionresult/{id:int}")] 
        public ActionResult<Autor> ResultadoActionResult(int id) // Mapear un parámetro a una variable "{id:int} -> int id" se denomina Model Binding
        // ActionResult<> permite retornar cualquier dato de tipo Autor o de tipo ActionResult.
        // Resulta que NotFound() hereda de ActionResult por lo que así puede ser devuelto.
        {
            var autor = context.Autores.FirstOrDefault(x => x.Id == id);

            if (autor == null)
            {
                return NotFound(); // Ahora NotFound() sí puede ser devuelto.
            }
            return autor;
        }

        //---------- GET ---------- api/autores/iactionresult/{id}

        [HttpGet("iactionresult/{id:int}")]
        public IActionResult ResultadoIActionResult(int id)
        // IActionResult permite retornar cualquier dato de tipo ActionResult o de cualquier tipo mientras esté dentro de la función Ok().
        {
            var autor = context.Autores.FirstOrDefault(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            //return Ok(autor);
            return Ok(7); // El problema es que el dato puede ser de cualquier tipo, el único requisito es que se encuentre en OK().
                          // Es mejor utilizar ActionResult<> para tener mayor control sobre el dato devuelto.
        }

        //---------- GET ---------- api/autores/listado || /listado

        [HttpGet("listado")] //Ahora la ruta será "/api/autores/listado"
        [HttpGet("/listado")] //Ahora la ruta será "listado"
        // Puede haber múltiples rutas para un mismo atributo de endpoint
        [ServiceFilter(typeof(MiFiltroDeAccion))] //Filtro de tubería personalizado
        public async Task<ActionResult<List<Autor>>> ListadoAutores() //Este endpoint devuelve la lista con todos los datos de la tabla Autor. La ruta es "api/autores"
        {

            // |---------|
            // | LOGGERS | 
            // |---------|

            //Tipos (menor a mayor severidad):
            // - Trace
            // - Debug
            // - Information
            // - Warning
            // - Error
            // - Critical

            //throw new NotImplementedException();
            logger.LogInformation("Estamos obteniendo los autores"); 
            servicio.RealizarTarea();
            return await context.Autores.Include(x => x.Libros).ToListAsync();
        }

        //---------- GET ---------- api/autores/primero

        [HttpGet("primero")] // -> Atributo del endpoint
                             // Este es otro endpoint GET cuya ruta va a ser "api/autores/primero". Si no se indica ninguna cadena dará error ya que habrá dos endpoints GET para la misma ruta
        public async Task<ActionResult<Autor>> PrimerAutor([FromHeader] int miValor, [FromQuery] string nombre, [FromQuery] string apellido) 
            // Este endpoint devuelve el primer autor de la tabla Autor o un null si no hay ningún valor.
            // El atributo [FromHeader] indica que los datos serán tomados del header y mapeados en 'miValor'.
            // El atributo [FromQuery] indica que los datos serán tomados de una Query String y mapeados en 'nombre'.
            // Una Query String es una sentencia que se coloca a continuación de la ruta y contiene datos de consulta
            // Ej de Query String: "api/autores/primero?nombre=felipe&apellido=gavilan"
            // "?" -> inicio del query
            // "&" -> concatenación de consulta
        {
            return await context.Autores.FirstOrDefaultAsync();
        }

        //---------- GET ---------- api/autores/{id}

        [HttpGet("{id:int}")] // La ruta será "api/autores/{id}" donde id es una variable (ej: api/autores/1 -> buscará el autor con id = 1)
                              // La restricción {:int} indica que solo se pueden introducir datos de tipo int, de lo contrario devuelve un 404.
        public async Task<ActionResult<Autor>> Get([FromRoute]int id) // Este endpoint recibe por parámetro el {id}.
                                                                      // Existen también los atributos a nivel de parámetros. 
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id); 
            //Devuelve x, donde el Id de x ha de ser igual al parámetro {id} que recibe el endpoint.
            //Si no encuentra ningún autor con id = {id}, devolverá null.

            if (autor == null)
            {
                return NotFound();
            }
            //Para evitar devolver valores null se guarda el dato devuelto en una variable y con un if devolvemos un error 404 en vez del null.

            return autor; 
        }

        //---------- GET ---------- api/autores/{nombre}/{param2}/{param3} || api/autores/{nombre}/{param2/3} || api/autores/{nombre}

        [HttpGet("{nombre}/{param2?}/{param3=persona}")] 
        // No se puede colocar una restricción de tipo string.
        // Se pueden colocar infinitos parámetros.
        // Al colocar "?" después de param2 incicamos que es opcional.
        // Al igualar param3 a "persona" le estamos dando un valor predeterminado.
        // En este caso la ruta sería "api/autores/Alex/martillo" o "api/autores/Alex" o "api/autores/Alex/martillo/animal".
        public async Task<ActionResult<Autor>> Get(string nombre, string param2, string param3) // Este endpoint recibe por parámetro el {nombre} de tipo string
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Nombre.Contains(nombre)); 
            // En vez de igualarlo se utiliza la función Contains() que muestra todos los resultados que contiene esa cadena

            if (autor == null)
            {
                return NotFound();
            }

            return autor;
        }

        //---------- GET ---------- api/autores/GUID

        [HttpGet("GUID")]
        //[ResponseCache(Duration = 10)] 
        //Filtro de tubería a nivel de acción -> indica que la respuesta dada a esta petición se va a guardar en el caché durante 10s. Durante los próximos 10s a una respuesta, todas las respuestas será la almacenada en la caché.
        // Se utiliza para datos que no van a cambiar mucho.
        // Aumenta la escalabilidad de la app.
        [ServiceFilter(typeof(MiFiltroDeAccion))] //Filtro de tubería personalizado
        public ActionResult ObtenerGuids()
        {
            return Ok(new
            {
                // Transient -> van a cambiar con cada ejecución y cada uno es diferente
                // ---------------------------------------------------------------------

                AutoresControllerTransient = servicioTransient.guid, // Retorna el guid a través del ServicioB
                ServicioA_Transient = servicio.ObtenerTransient(), // Retorna el guid a través del ServicioA

                // Scoped -> van a cambiar con cada ejecución pero son iguales los 2
                // -----------------------------------------------------------------

                AutoresControllerScoped = servicioScoped.guid, // Retorna el guid a través del ServicioB
                ServicioA_Scoped = servicio.ObtenerScoped(), // Retorna el guid a través del ServicioA

                // Singleton -> van a mantenerse durante todas las ejecuciones y son iguales los 2
                // -------------------------------------------------------------------------------

                AutoresControllerSingleton = servicioSingleton.guid, // Retorna el guid a través del ServicioB
                ServicioA_Singleton = servicio.ObtenerSignleton() // Retorna el guid a través del ServicioA
            });
        }

        //---------- POST ----------

        [HttpPost] //-> Atributo del endpoint
                   //Si se ejecuta una petición POST hacia este controlador, se ejecutará el código que hay dentro de este bloque
        public async Task<ActionResult> Post([FromBody]Autor autor)
        {

            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autor.Nombre );

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autor.Nombre}");
            }

            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        //---------- PUT ----------

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

        //---------- DELETE ----------

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
