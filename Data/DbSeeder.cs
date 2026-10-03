using AutoGestionAPI.Models;
using Microsoft.EntityFrameworkCore; // Necesario para el AnyAsync

namespace AutoGestionAPI.Data
{
    public static class DbSeeder
    {
        // 1. CAMBIO CLAVE: Ahora es "async Task" y usa tu Contexto real (Ajustá IscgbContext si se llama distinto)
        public static async Task Inicializar(TuDbContext context)
        {
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Rol = "Director" },    // ID 1
                    new Role { Rol = "Secretario" },  // ID 2
                    new Role { Rol = "Docente" },     // ID 3
                    new Role { Rol = "Alumno" }       // ID 4
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            if (!context.Pais.Any())
            {
                var argentina = new Pai { Nombre = "Argentina" };
                var uruguay = new Pai { Nombre = "Uruguay" };
                var peru = new Pai { Nombre = "Perú" };
                var venezuela = new Pai { Nombre = "Venezuela" };

                context.Pais.AddRange(argentina, uruguay, peru, venezuela);
                context.SaveChanges();

                var provincias = new List<Provincium>
                {
                    // Provincias de Argentina
                    new Provincium { Nombre = "Córdoba", IdPais = argentina.IdPais },
                    new Provincium { Nombre = "Buenos Aires", IdPais = argentina.IdPais },
                    new Provincium { Nombre = "San Luis", IdPais = argentina.IdPais },
                    new Provincium { Nombre = "La Pampa", IdPais = argentina.IdPais },
                    new Provincium { Nombre = "Río Negro", IdPais = argentina.IdPais },
                    
                    // Departamentos de Uruguay
                    new Provincium { Nombre = "Montevideo", IdPais = uruguay.IdPais },
                    new Provincium { Nombre = "San José", IdPais = uruguay.IdPais },
                    
                    // Provincias de Peru
                    new Provincium { Nombre = "Lima", IdPais = peru.IdPais },
                    new Provincium { Nombre = "Cusco", IdPais = peru.IdPais },

                    // Estados de Venezuela
                    new Provincium { Nombre = "Miranda", IdPais = venezuela.IdPais },
                    new Provincium { Nombre = "Zulia", IdPais = venezuela.IdPais }
                };

                context.Provincia.AddRange(provincias);
                context.SaveChanges();
            }

            if (!context.TiposDocumentos.Any())
            {
                var tiposDocumentos = new List<TiposDocumento>
                {
                    new TiposDocumento { NombreDocumento = "DNI FRENTE" },
                    new TiposDocumento { NombreDocumento = "DNI DORSO" },
                    new TiposDocumento { NombreDocumento = "Curriculum Vitae" },
                    new TiposDocumento { NombreDocumento = "Titulo Profesional" },
                    new TiposDocumento { NombreDocumento = "DDJJ Incompatibilidad Horaria" },
                    new TiposDocumento { NombreDocumento = "Apto Medico" },
                    new TiposDocumento { NombreDocumento = "Apto Psicologico" },
                    new TiposDocumento { NombreDocumento = "Antecedentes Penales" },
                    new TiposDocumento { NombreDocumento = "Ley 9680" },
                    new TiposDocumento { NombreDocumento = "Certificado De Domicilio" }
                };

                context.TiposDocumentos.AddRange(tiposDocumentos);
                context.SaveChanges();
            }

            if (!context.RolesTiposDocumentos.Any())
            {
                var rolesDocumentos = new List<RolesTiposDocumento>
                {
                    // Documentos para Docentes (Rol 3)
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 1, Obligatorio = false },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 2, Obligatorio = false },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 3, Obligatorio = false },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 4, Obligatorio = true },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 5, Obligatorio = true },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 6, Obligatorio = true },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 7, Obligatorio = false },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 8, Obligatorio = true },
                    new RolesTiposDocumento { IdRol = 3, IdTipoDoc = 9, Obligatorio = true },

                    // Documentos para Alumnos (Rol 4)
                    new RolesTiposDocumento { IdRol = 4, IdTipoDoc = 1, Obligatorio = false },
                    new RolesTiposDocumento { IdRol = 4, IdTipoDoc = 5, Obligatorio = true },
                    new RolesTiposDocumento { IdRol = 4, IdTipoDoc = 7, Obligatorio = false },
                    new RolesTiposDocumento { IdRol = 4, IdTipoDoc = 10, Obligatorio = false }
                };

                context.RolesTiposDocumentos.AddRange(rolesDocumentos);
                context.SaveChanges();
            }

            if (!await context.Comisions.AnyAsync())
            {
                var comisiones = new List<Comision>
                {
                    new Comision { Comision1 = "A" },
                    new Comision { Comision1 = "B" }
                };

                await context.Comisions.AddRangeAsync(comisiones);
                await context.SaveChangesAsync();
            }
        }
    }
}