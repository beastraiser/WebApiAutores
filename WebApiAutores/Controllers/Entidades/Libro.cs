﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Controllers.Entidades
{
    public class Libro
    {
        public int Id{ get; set; }

        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250)]
        public string Titulo { get; set; }

        //public int AutorId { get; set; }

        //public Autor Autor { get; set; }
    }
}
