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
                List<Role> roles = new List<Role>
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
                Pai argentina = new Pai { Nombre = "Argentina" };
                Pai uruguay = new Pai { Nombre = "Uruguay" };
                Pai peru = new Pai { Nombre = "Perú" };
                Pai venezuela = new Pai { Nombre = "Venezuela" };

                context.Pais.AddRange(argentina, uruguay, peru, venezuela);
                context.SaveChanges();

                List<Provincium> provincias = new List<Provincium>
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

            await SincronizarDocumentacionAsync(context);

            // Años de las materias escritos de distintas formas ("1° año", "primer año")
            // se unifican ("Primer Año"), porque el límite de exámenes se cuenta por año.
            // Los que no se reconocen quedan como están, para revisarlos a mano.
            List<Materia> materias = await context.Materias.Where(m => m.Curso != null).ToListAsync();
            foreach (Materia materia in materias)
            {
                string? curso = AutoGestionAPI.Services.CursosMateria.Normalizar(materia.Curso);
                if (curso != null && curso != materia.Curso)
                    materia.Curso = curso;
            }
            await context.SaveChangesAsync();

            // Tipos de examen del calendario. Están definidos en el modelo (HasData),
            // pero en algunas bases quedaron vacíos y sin ellos no se puede registrar
            // ningún examen. Se agregan los que falten, en este orden (ids 1, 2 y 3).
            foreach (string tipo in new[] { "Parcial", "Recuperatorio", "Final" })
            {
                if (!await context.TipoExamen.AnyAsync(t => t.TipoExamen == tipo))
                {
                    context.TipoExamen.Add(new TipoExaman { TipoExamen = tipo });
                    await context.SaveChangesAsync();
                }
            }

            if (!await context.Comisions.AnyAsync())
            {
                List<Comision> comisiones = new List<Comision>
                {
                    new Comision { Comision1 = "A" },
                    new Comision { Comision1 = "B" }
                };

                await context.Comisions.AddRangeAsync(comisiones);
                await context.SaveChangesAsync();
            }
        }

        // ------------------------------------------------------------------
        // DOCUMENTACIÓN EXIGIDA POR ROL
        // ------------------------------------------------------------------
        // Esta es la ÚNICA fuente de verdad de qué documentos se le piden a cada
        // rol. A diferencia del resto del seeder, no se carga solo con la base
        // vacía: se SINCRONIZA en cada arranque. Si se cambia esta lista, al
        // correr "dotnet run" la base queda igual a ella (agrega lo que falta,
        // corrige obligatorio/anual y quita lo que ya no se pide). Los archivos
        // ya subidos no se tocan.
        //
        // Anual = true: el documento se vuelve a presentar una vez por año.
        // Anual = false: se presenta una sola vez.

        private const string Dni = "DNI";
        private const string CurriculumVitae = "Curriculum Vitae";
        private const string TituloProfesional = "Titulo Profesional";
        private const string AptoMedico = "Apto Medico";
        private const string AptoPsicologico = "Apto Psicologico";
        private const string Ley9680 = "Ley 9680";
        private const string CertificadoDomicilio = "Certificado De Domicilio";
        private const string AnaliticoSecundario = "Analitico Secundario";
        private const string PartidaNacimiento = "Partida De Nacimiento";
        private const string AntecedentesPenales = "Antecedentes Penales";

        private record DocumentoExigido(string Documento, bool Obligatorio, bool Anual);

        private static readonly Dictionary<string, DocumentoExigido[]> DocumentosPorRol = new()
        {
            ["Docente"] = new[]
            {
                new DocumentoExigido(Dni, Obligatorio: true, Anual: false),
                new DocumentoExigido(CurriculumVitae, Obligatorio: true, Anual: false),
                new DocumentoExigido(TituloProfesional, Obligatorio: true, Anual: false),
                new DocumentoExigido(AptoMedico, Obligatorio: true, Anual: true),
                new DocumentoExigido(AptoPsicologico, Obligatorio: true, Anual: true),
                new DocumentoExigido(Ley9680, Obligatorio: true, Anual: true),
                new DocumentoExigido(CertificadoDomicilio, Obligatorio: true, Anual: true),
            },
            ["Alumno"] = new[]
            {
                new DocumentoExigido(Dni, Obligatorio: true, Anual: false),
                new DocumentoExigido(AnaliticoSecundario, Obligatorio: true, Anual: false),
                new DocumentoExigido(PartidaNacimiento, Obligatorio: true, Anual: false),
                new DocumentoExigido(AntecedentesPenales, Obligatorio: true, Anual: false),
                new DocumentoExigido(AptoMedico, Obligatorio: true, Anual: true),
                new DocumentoExigido(AptoPsicologico, Obligatorio: true, Anual: true),
            },
        };

        private static async Task SincronizarDocumentacionAsync(TuDbContext context)
        {
            // Antes el DNI se pedía en dos archivos ("DNI FRENTE" y "DNI DORSO").
            // Ahora es uno solo: se renombra "DNI FRENTE" para que lo ya subido
            // siga contando como DNI.
            List<TiposDocumento> tipos = await context.TiposDocumentos.ToListAsync();
            TiposDocumento? dniFrente = tipos.FirstOrDefault(t => MismoNombre(t.NombreDocumento, "DNI FRENTE"));
            if (dniFrente != null && !tipos.Any(t => MismoNombre(t.NombreDocumento, Dni)))
                dniFrente.NombreDocumento = Dni;

            // 1. Que existan todos los tipos de documento de la lista
            IEnumerable<string> nombresExigidos = DocumentosPorRol.Values
                .SelectMany(docs => docs.Select(d => d.Documento))
                .Distinct();
            foreach (string nombre in nombresExigidos)
            {
                if (!tipos.Any(t => MismoNombre(t.NombreDocumento, nombre)))
                {
                    TiposDocumento nuevo = new TiposDocumento { NombreDocumento = nombre };
                    context.TiposDocumentos.Add(nuevo);
                    tipos.Add(nuevo);
                }
            }
            await context.SaveChangesAsync();

            // 2. Que cada rol tenga exactamente sus documentos, con los valores de la lista
            List<Role> roles = await context.Roles.ToListAsync();
            List<RolesTiposDocumento> reglas = await context.RolesTiposDocumentos.ToListAsync();

            foreach (KeyValuePair<string, DocumentoExigido[]> rolYDocumentos in DocumentosPorRol)
            {
                Role? rol = roles.FirstOrDefault(r => r.Rol == rolYDocumentos.Key);
                if (rol == null) continue;

                List<int> idsExigidos = new List<int>();
                foreach (DocumentoExigido exigido in rolYDocumentos.Value)
                {
                    int idTipoDoc = tipos.First(t => MismoNombre(t.NombreDocumento, exigido.Documento)).IdTipoDoc;
                    idsExigidos.Add(idTipoDoc);

                    RolesTiposDocumento? regla = reglas.FirstOrDefault(r => r.IdRol == rol.IdRol && r.IdTipoDoc == idTipoDoc);
                    if (regla == null)
                    {
                        context.RolesTiposDocumentos.Add(new RolesTiposDocumento
                        {
                            IdRol = rol.IdRol,
                            IdTipoDoc = idTipoDoc,
                            Obligatorio = exigido.Obligatorio,
                            Anual = exigido.Anual,
                        });
                    }
                    else
                    {
                        regla.Obligatorio = exigido.Obligatorio;
                        regla.Anual = exigido.Anual;
                    }
                }

                // Lo que este rol tenía configurado y ya no está en la lista, se quita
                context.RolesTiposDocumentos.RemoveRange(
                    reglas.Where(r => r.IdRol == rol.IdRol && !idsExigidos.Contains(r.IdTipoDoc)));
            }
            await context.SaveChangesAsync();

            // 3. "DNI DORSO" ya no se usa: se borra del catálogo si nadie lo tiene subido
            TiposDocumento? dniDorso = tipos.FirstOrDefault(t => MismoNombre(t.NombreDocumento, "DNI DORSO"));
            if (dniDorso != null
                && !await context.Legajos.AnyAsync(l => l.IdTipoDoc == dniDorso.IdTipoDoc)
                && !await context.RolesTiposDocumentos.AnyAsync(r => r.IdTipoDoc == dniDorso.IdTipoDoc))
            {
                context.TiposDocumentos.Remove(dniDorso);
                await context.SaveChangesAsync();
            }
        }

        private static bool MismoNombre(string a, string b)
        {
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}