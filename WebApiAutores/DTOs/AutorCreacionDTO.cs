using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class AutorCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")] // Indica que el campo Nombre es obligatorio.
                                                               // El parámetro ErrorMesage deja personalizar un mensaje de error.
                                                               // {0} tomará el valor de "Nombre".
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe de tener más de {1} carácteres")]
        [PrimeraLetraMayuscula] // -> Es una validacion personalizada. Se encuentra en la carpeta Validaciones.
        // Pueden existir infinitas condiciones
        public string Nombre { get; set; }
    }
}
