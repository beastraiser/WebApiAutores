using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using WebApiAutores.Controllers;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //var autoresController = new AutoresController(new ApplicationDbContext(null), new ServicioA(new Logger(--dependencias de Logger--)));
            //autoresController.Get();

            // Esto es un mero ejemplo para visualizar como se instancian las dependencias.
            // No es una manera correcta de trabajar ya que crea una jerarquía de dependecias muy caótica (X depende de Y que depende de Z que depende de ...).
            // Lo que se utiliza es un sistema de inyección de dependencias.

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // |--------------------------------------|
        // | SISTEMA DE INYECCIÓN DE DEPENDENCIAS | 
        // |--------------------------------------|

        public void ConfigureServices(IServiceCollection services)
        {
        // En este bloque se configuran todos los servicios de manera que no haya que instanciarlos cuando se necesiten.
        // Servicio -> Resolución de una dependencia configurada en el Sistema de Inyección de Dependencias

            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltroDeExcepcion)); // Linea para registrar los filtros personalizados a nivel global
            }).AddJsonOptions(x => 
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

            // Servicio para AplicationDBContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddAutoMapper(typeof(Startup));


            // |--------------------|
            // | TIPOS DE SERVICIOS |
            // |--------------------|

            //Transient
            //---------
            //services.AddTransient<ServicioTransient>();
            //services.AddTransient<ServicioA>(); // También se puede configurar solamente para las clases, sin su interfaz
            // Cada vez que una clase requiera un IServicio, se le pasa una nueva instancia de la clase ServicioA.
            // Entre distintas peticiones http se proporcionarán diferenetes instancias.
            // Ejemplo: validación de primera letra mayúscula

            //Scoped
            //------
            //services.AddScoped<ServicioScoped>();
            // La instancia se ServicioA va a ser la misma en todo el contexto. Aumenta su tiempo de vida.
            // Entre distintas peticiones http se proporcionarán diferenetes instancias.
            // Ejemplo: AddDbContext

            //Singleton
            //---------
            //services.AddSingleton<ServicioSingleton>();
            // La instancia se ServicioA va a ser la misma siempre, incluso entre diferentes peticiones http.
            // Ejemplo: capas de cache

            //services.AddTransient<IServicio, ServicioA>(); // Utilizado en el ejemplo de como funciona cada tipo de servicio

            //services.AddTransient<MiFiltroDeAccion>(); // Servicio necesario para utilizar el filtro personalizado de acción.

            //services.AddHostedService<EscribirEnArchivo>();

            //services.AddResponseCaching(); // Sevicio necesario para poder usar caché en nuestra app.

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(); // Servicio necesario para hacer filtros de autenticación

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIAutores",  Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILogger<Startup> logger)
        {

            // |------------|
            // | MIDDLEWARE |
            // |------------|

            // Los middleware se configuran en el método Configure() de la clase Startup.
            // Tubería -> múltiples procesos conectados de tal forma que la salida de uno de los procesos es la entrada del siguiente.
            // Middleware -> cada uno de los procesos que conforman una tubería.
            // Reciben una petición http y procesan algun tipo de resultado.
            // El orden de los middlewares es importante.
            // El código siguiente conforma la tubería. Cada método es un middleware y se ejecutan en orden.

            // |---------|
            // | FILTROS |
            // |---------|

            // Los filtros se utilizan para correr un determinado código en ciertos momentos de la ejecución de la tubería

            // Tipos:
            // - Autorización -> un usuario puede o no consumir una acción determinada
            // - Recursos -> se ejecutan despues de la etapa de autorización. Para validaciones generales, implementar una capa de caché o detener la tubería de filtros
            // - Acción -> se ejecutan justo antes y despues de una acción. Para manipular los argumentos enviados a una acción o la info retornada por los mismos
            // - Excepción -> se ejecutan cuando hubo una excepción no atrapada en un try/catch durante la ejecución de una acción, un filtro de acción, creación de un                    controlador y durante el binding de modelo.
            // - Resultado -> se ejecutan antes y después de la ejecución de un ActionResult<>

            // Alcance:
            // - Nivel de acción
            // - Nivel de controlador
            // - Nivel de global


            // Este middleware recoge todas las respuestas que devuelve nuestra api.
            app.UseLoguearRespuestaHTTP();

            // Este es el primer middleware por tanto se ejecuta el primero.
            // Establece un mapa para la tubería, una bifurcación.
            // Si la ruta es /ruta1, se ejecuta lo de dentro, sino sigue el curso lineal.
            //app.Map("/ruta1", app =>
            //{
            //    // Este midleware intercepta el resto de procesos.
            //    app.Run(async contexto =>
            //    {
            //        await contexto.Response.WriteAsync("Estoy interceptando la tubería");
            //    });
            //});
            
            if (env.IsDevelopment()) // IsDevelopment -> utilidad que devuelve si estamos o no en desarrollo. No es un middleware.
                                     // Los que no se encuentran en este if se ejecutarán en producción.
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseResponseCaching(); //Middleware por defecto para usar el caché

            app.UseAuthorization(); //Middleware por defecto para usar la autenticación

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
