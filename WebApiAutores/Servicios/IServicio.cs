namespace WebApiAutores.Servicios
{
    public interface IServicio
    {
        Guid ObtenerScoped();
        Guid ObtenerSignleton();
        Guid ObtenerTransient();
        void RealizarTarea();
    }

    // Aquí se implementan las clases que contiene cada tipo de servicio
    public class ServicioA : IServicio
    {
        private readonly ILogger<ServicioA> logger;
        private readonly ServicioTransient servicioTransient;
        private readonly ServicioScoped servicioScoped;
        private readonly ServicioSingleton servicioSingleton;

        public ServicioA(ILogger<ServicioA> logger, ServicioTransient servicioTransient, ServicioScoped servicioScoped, ServicioSingleton servicioSingleton)
        {
            this.logger = logger;
            this.servicioTransient = servicioTransient;
            this.servicioScoped = servicioScoped;
            this.servicioSingleton = servicioSingleton;
        }

        // Aquí se definen los métodos que retornan los código generados de cada tipo de servicio
        public Guid ObtenerTransient() { return servicioTransient.guid; } 
        public Guid ObtenerScoped() { return servicioScoped.guid; }
        public Guid ObtenerSignleton() { return servicioSingleton.guid; }

        public void RealizarTarea()
        {
        }
    }

    public class ServicioB : IServicio
    {
        public Guid ObtenerScoped()
        {
            throw new NotImplementedException();
        }

        public Guid ObtenerSignleton()
        {
            throw new NotImplementedException();
        }

        public Guid ObtenerTransient()
        {
            throw new NotImplementedException();
        }

        public void RealizarTarea()
        {
        }
    }

    // Aquí se definen las clases que contienen cada tipo de servicio
    // guid -> codigo aleatorio autogenerado
    public class ServicioTransient 
    {
        public Guid guid = Guid.NewGuid();
    }

    public class ServicioScoped
    {
        public Guid guid = Guid.NewGuid();
    }

    public class ServicioSingleton
    {
        public Guid guid = Guid.NewGuid();
    }
}
