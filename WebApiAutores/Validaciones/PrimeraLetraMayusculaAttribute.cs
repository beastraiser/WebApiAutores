using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Validaciones
{
    //---------- Estructura para crear validaciones personalizadas ----------
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute // ValidationAttribute -> contexto del que tiene que heredar
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) // "value" será el valor del campo y "validationContext" serán todos los campos
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) //La segunda condición evalúa si valor es null o está vacío
            {
                return ValidationResult.Success;
            }

            var primeraLetra = value.ToString()[0].ToString(); // Almacena la primera letra de value en "primeraLetra"

            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayúscula");
            }

            return ValidationResult.Success;
        }
    }
}
