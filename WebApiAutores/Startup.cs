using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using WebApiAutores.Controllers;
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

            services.AddControllers().AddJsonOptions(x => 
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddTransient<IServicio, ServicioA>(); // Utilizado en el ejemplo de como funciona cada tipo de servicio

            // |--------------------|
            // | TIPOS DE SERVICIOS |
            // |--------------------|

            //Transient
            //---------
            services.AddTransient<ServicioTransient>();
            //services.AddTransient<ServicioA>(); // También se puede configurar solamente para las clases, sin su interfaz
            // Cada vez que una clase requiera un IServicio, se le pasa una nueva instancia de la clase ServicioA.
            // Entre distintas peticiones http se proporcionarán diferenetes instancias.
            // Ejemplo: validación de primera letra mayúscula

            //Scoped
            //------
            services.AddScoped<ServicioScoped>();
            // La instancia se ServicioA va a ser la misma en todo el contexto. Aumenta su tiempo de vida.
            // Entre distintas peticiones http se proporcionarán diferenetes instancias.
            // Ejemplo: AddDbContext

            //Singleton
            //---------
            services.AddSingleton<ServicioSingleton>();
            // La instancia se ServicioA va a ser la misma siempre, incluso entre diferentes peticiones http.
            // Ejemplo: capas de cache

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIAutores",  Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
