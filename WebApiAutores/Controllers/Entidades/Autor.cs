using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Controllers.Entidades
{
    public class Autor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")] // Indica que el campo Nombre es obligatorio.
                                                               // El parámetro ErrorMesage deja personalizar un mensaje de error.
                                                               // {0} tomará el valor de "Nombre".
        [StringLength(maximumLength: 4, ErrorMessage = "El campo {0} no debe de tener más de {1} carácteres")]
        [PrimeraLetraMayuscula] // Es una validacion personalizada. Se encuentra en la carpeta Validaciones
        // Pueden existir infinitas condiciones
        public string Nombre { get; set; }

        [Range(18, 120)]
        [NotMapped] // Indica que la propiedad no va a ser parte de la tabla
        public int Edad { get; set; }

        [CreditCard]
        [NotMapped]
        public string TarjetaDeCredito { get; set; }

        [Url]
        [NotMapped]
        public string URL { get; set; }

        public List<Libro> Libros { get; set; }
    }
}
