# ISCGB Backend

API REST para el sistema de gestión del Instituto Superior Cura Gabriel Brochero
(ISCGB).

## Puesta en marcha

### Requisitos

- .NET SDK compatible con `AutoGestionAPI.csproj`.
- SQL Server local con la base `Autogestion_Docente`.
- Angular u otro frontend ejecutándose en `http://localhost:4200` (origen
  permitido por CORS).

### Ejecutar

```bash
dotnet restore
dotnet run
```

En desarrollo, la API queda disponible en:

- HTTP: `http://localhost:5231`
- HTTPS: `https://localhost:7055`
- Swagger: `http://localhost:5231/swagger` o
  `https://localhost:7055/swagger`

El frontend debe configurar una única variable para la URL base, por ejemplo:

```ts
export const API_URL = 'http://localhost:5231/api';
```

Todas las rutas indicadas debajo parten de `API_URL`.

## Autenticación

### Iniciar sesión

`POST /Auth/login`

```json
{
  "dni": "12345678",
  "password": "contraseña"
}
```

La respuesta exitosa contiene `token`, los datos del usuario y `roles`:

```json
{
  "token": "eyJ...",
  "idUsuario": 1,
  "usuario": "Nombre Apellido",
  "dni": "12345678",
  "roles": [
    { "idRol": 3, "nombreRol": "Docente" }
  ]
}
```

Guardar el token y enviarlo en cada request protegida:

```http
Authorization: Bearer eyJ...
```

Roles disponibles:

| ID | Rol |
| --- | --- |
| 1 | Director |
| 2 | Secretario |
| 3 | Docente |
| 4 | Alumno |

Si la respuesta es `401`, redirigir al login. Los errores normalmente tienen
el formato `{ "message": "..." }`, aunque algunos endpoints devuelven el
mensaje como texto.

## Endpoints para el frontend

### Usuarios y autenticación

| Método | Ruta | Uso |
| --- | --- | --- |
| `POST` | `/Auth/login` | Iniciar sesión. |
| `POST` | `/Auth/crear-usuario-prueba` | Crear usuario de prueba. Solo desarrollo. |
| `POST` | `/UsuariosAdmin/alta` | Crear usuario y enviar enlace para configurar contraseña. |
| `POST` | `/UsuariosAdmin/establecer-password` | Configurar contraseña con el token recibido por correo. |
| `PUT` | `/UsuariosAdmin/modificar/{id}` | Modificar datos y roles de un usuario. |
| `PUT` | `/UsuariosAdmin/baja/{id}` | Dar de baja un usuario. |
| `PUT` | `/UsuariosAdmin/alta/{id}` | Reactivar un usuario. |
| `GET` | `/Usuarios/{id}` | Obtener el detalle de un usuario. |
| `GET` | `/Usuarios?rol=Docente&estado=true&pagina=1&registrosPorPagina=10` | Listar usuarios paginados. |

Para alta o modificación, el cuerpo JSON usa los campos de
`CargaUsuarioDto`:

```json
{
  "nombre": "Ana",
  "apellido": "Pérez",
  "dni": "12345678",
  "cuil": "27-12345678-9",
  "email": "ana@example.com",
  "genero": "F",
  "direccion": "Calle 123",
  "telefono": "3515555555",
  "idProvincia": 1,
  "fechaNac": "1990-01-31",
  "contactoEmergencia": "Juan Pérez",
  "telefonoEmergencia": "3514444444",
  "afiliacionEmergencia": "Familiar",
  "idsRoles": [3],
  "esDirectorSuplente": false
}
```

Para configurar una contraseña:

```json
{
  "token": "token-recibido-por-correo",
  "nuevaPassword": "NuevaClave123"
}
```

La lista de usuarios devuelve `Paginacion` y `Datos`. Si no hay resultados,
`GET /Usuarios` responde `404`.

### Ubicaciones, búsqueda y materias

| Método | Ruta | Uso |
| --- | --- | --- |
| `GET` | `/Ubicaciones/paises` | Listar países. |
| `GET` | `/Ubicaciones/paises/{idPais}/provincias` | Listar provincias de un país. |
| `GET` | `/Buscador/global?termino=texto` | Buscar usuarios, materias y justificativos. Con menos de 2 caracteres devuelve una lista vacía. |
| `GET` | `/Asignaciones/materias-disponibles` | Materias para un selector. |
| `GET` | `/Asignaciones/docentes-disponibles` | Docentes para un selector. |
| `GET` | `/Asignaciones/comisiones-disponibles` | Comisiones para un selector. |
| `POST` | `/Asignaciones/asignar` | Asignar una materia a un docente y comisión. |
| `POST` | `/Asignaciones/cargar-materia` | Crear una materia. |

`POST /Asignaciones/asignar`:

```json
{
  "idDocente": 1,
  "idMateria": 2,
  "idComision": 3
}
```

Los tres endpoints de listas de asignaciones devuelven `{ "data": [...] }`.

### Legajos y documentación

| Método | Ruta | Uso |
| --- | --- | --- |
| `POST` | `/Legajos` | Subir un documento. Requiere `multipart/form-data`. |
| `GET` | `/Legajos/usuario/{idUsuario}` | Consultar documentos de un usuario. |
| `PUT` | `/Legajos/auditar/{idLegajo}?idUsuarioAuditor={id}` | Aprobar o rechazar un documento. |
| `GET` | `/Legajos/requeridos-por-rol/{idRol}` | Obtener documentos requeridos por rol. |
| `GET` | `/Legajos/pendientes` | Listar legajos pendientes. |
| `GET` | `/Legajos/resumen-estado` | Resumen de estados. |
| `GET` | `/Legajos/{idUsuario}/faltantes` | Documentación faltante de un usuario. |
| `GET` | `/Legajos/aprobados` | Listar legajos aprobados. |

Para `POST /Legajos`, enviar estos campos en `FormData`:

```ts
const form = new FormData();
form.append('idUsuario', String(idUsuario));
form.append('idTipoDoc', String(idTipoDoc));
form.append('fechaVencimiento', '2026-12-31'); // opcional
form.append('presentadoFisico', 'false');
form.append('archivo', archivo);
```

No establecer manualmente `Content-Type`: el navegador agrega el boundary
necesario para `multipart/form-data`.

Para auditar un documento:

```json
{
  "estado": "Aprobado",
  "comentario": "Documentación correcta"
}
```

### Justificativos

| Método | Ruta | Uso |
| --- | --- | --- |
| `POST` | `/Justificativos/cargar` | Cargar una inasistencia. Requiere `multipart/form-data`. |
| `PUT` | `/Justificativos/auditar/{idJustificativo}` | Auditar un justificativo. |
| `GET` | `/Justificativos/pendientes` | Listar justificativos pendientes. |
| `GET` | `/Justificativos/{idUsuario}/justificativos` | Listar justificativos de un usuario. |
| `GET` | `/Justificativos/todos` | Listar todos los justificativos. |

Campos de `FormData` para cargar un justificativo:

```ts
const form = new FormData();
form.append('idUsuario', String(idUsuario));
form.append('tipoInasistencia', 'Enfermedad');
form.append('notaAdicional', 'Observación opcional');
form.append('fechaInasistenciaInicio', '2026-10-01');
form.append('fechaInasistenciaFin', '2026-10-02');
form.append('documentoPdf', archivoPdf); // obligatorio salvo "Causas Personales"
```

### Reconocimiento de saberes

Estas rutas requieren JWT. La solicitud del alumno requiere además el rol
`Alumno`; las consultas de secretaría requieren `Secretario`.

| Método | Ruta | Rol | Uso |
| --- | --- | --- | --- |
| `POST` | `/ReconocimientoSaberes/solicitar` | Alumno | Enviar solicitud con dos PDFs. |
| `GET` | `/ReconocimientoSaberes/recibirSolicitudReconocimiento` | Secretario | Listar solicitudes pendientes. |
| `GET` | `/ReconocimientoSaberes/{id}` | Secretario | Ver el detalle de una solicitud. |
| `GET` | `/ReconocimientoSaberes/{id}/programa` | Secretario | Descargar el programa PDF. |
| `GET` | `/ReconocimientoSaberes/{id}/analitico` | Secretario | Descargar el analítico PDF. |

Para solicitar reconocimiento, usar `FormData` con:

```ts
const form = new FormData();
form.append('idMateria', String(idMateria));
form.append('comentario', 'Comentario opcional');
form.append('programaPdf', programaPdf);
form.append('analiticoPdf', analiticoPdf);
```

Ambos archivos deben ser PDF y pesar como máximo 10 MB.

### Programas y certificados

| Método | Ruta | Uso |
| --- | --- | --- |
| `POST` | `/ProgramasMateria` | Crear un programa de materia. |
| `GET` | `/ProgramasMateria/{idPrograma}/pdf` | Descargar el programa en PDF. |
| `GET` | `/ProgramasMateria/contexto-docente/{idUsuario}` | Obtener contexto del docente. |
| `GET` | `/Certificados/alumno-regular` | Descargar certificado de alumno regular. |
| `GET` | `/Certificados/alumno-regular-horario` | Descargar certificado con horario. |

Los endpoints de certificados requieren JWT y devuelven un archivo; el
frontend debe procesar la respuesta como `blob`.

## Archivos y URLs

Los archivos guardados se sirven como contenido estático. Las rutas relativas
devueltas por la API, por ejemplo `/uploads/legajos/archivo.pdf`, deben
completarse con la URL del backend:

```ts
const urlArchivo = `${API_URL.replace('/api', '')}${rutaRelativa}`;
```

Para PDFs protegidos por rol, usar una solicitud autenticada con `HttpClient`
y crear un `Blob` en lugar de abrir directamente la URL en una pestaña.

## Manejo recomendado en el frontend

1. Centralizar `API_URL` en un environment.
2. Usar un interceptor HTTP para agregar `Authorization: Bearer <token>`.
3. Guardar `token`, `idUsuario` y `roles` después del login.
4. Proteger vistas con guards según `nombreRol`.
5. Usar `FormData` para cargas de archivos y no fijar manualmente el header
   `Content-Type`.
6. Mostrar `message`, `mensaje` o el texto recibido cuando la API informa un
   error o resultado de una operación.
7. Consultar Swagger para ver los modelos y respuestas completos:
   `/swagger`.

## Contribuir

1. Crear una rama para el cambio.
2. Implementar y probar la modificación.
3. Ejecutar `dotnet build`.
4. Crear un commit descriptivo.
5. Subir la rama y abrir un Pull Request.
