using AutoMapper;
using WebApiAutores.Controllers.Entidades;
using WebApiAutores.DTOs;

namespace WebApiAutores.Utilidades
{
    // Aquí se configura el AutoMapper
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AutorCreacionDTO, Autor>(); // Desde AutorCreacionDTO hasta Autor
            CreateMap<Autor, AutorDTO>();
        }
    }
}
