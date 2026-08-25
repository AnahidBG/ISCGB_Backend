USE [master]
GO
/****** Objeto: Database [Autogestion_Docente] Fecha de script: 19/8/2026 18:39:01 ******/
CREATE DATABASE [Autogestion_Docente]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Autogestion_Docente', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\Autogestion_Docente.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Autogestion_Docente_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\Autogestion_Docente_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Autogestion_Docente] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Autogestion_Docente].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Autogestion_Docente] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET ARITHABORT OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Autogestion_Docente] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Autogestion_Docente] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Autogestion_Docente] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Autogestion_Docente] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Autogestion_Docente] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Autogestion_Docente] SET  MULTI_USER 
GO
ALTER DATABASE [Autogestion_Docente] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Autogestion_Docente] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Autogestion_Docente] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Autogestion_Docente] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Autogestion_Docente] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [Autogestion_Docente] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [Autogestion_Docente] SET QUERY_STORE = ON
GO
ALTER DATABASE [Autogestion_Docente] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Autogestion_Docente]
GO
/****** Objeto: Table [dbo].[alumno_materia] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[alumno_materia](
	[id_alumno_materia] [int] IDENTITY(1,1) NOT NULL,
	[id_materia] [int] NOT NULL,
	[id_alumno] [int] NOT NULL,
	[id_comision] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_alumno_materia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Alumnos] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Alumnos](
	[id_alumno] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[cohorte] [varchar](50) NULL,
	[estado_academico] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_alumno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[comision] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[comision](
	[id_comision] [int] IDENTITY(1,1) NOT NULL,
	[comision] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_comision] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[contenidos] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[contenidos](
	[id_contenido] [int] IDENTITY(1,1) NOT NULL,
	[id_programa] [int] NOT NULL,
	[unidad] [int] NOT NULL,
	[titulo_unidad] [varchar](255) NULL,
	[contenido] [varchar](max) NULL,
	[bibliografia_obligatoria] [varchar](max) NULL,
	[bibliografia_complementaria] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_contenido] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[docente_materia] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[docente_materia](
	[id_docente_materia] [int] IDENTITY(1,1) NOT NULL,
	[id_materia] [int] NOT NULL,
	[id_docente] [int] NOT NULL,
	[id_comision] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_docente_materia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[docente_tipo_titulo] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[docente_tipo_titulo](
	[id_docente_tipo_titulo] [int] IDENTITY(1,1) NOT NULL,
	[id_docente] [int] NOT NULL,
	[id_tipo_titulo] [int] NOT NULL,
	[institucion] [varchar](150) NULL,
	[fecha_egreso] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_docente_tipo_titulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Docentes] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Docentes](
	[id_docente] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[director_suplente] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_docente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Examenes] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Examenes](
	[id_examen] [int] IDENTITY(1,1) NOT NULL,
	[id_materia] [int] NOT NULL,
	[id_comision] [int] NOT NULL,
	[id_tipo_examen] [int] NOT NULL,
	[fecha] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_examen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Justificativos] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Justificativos](
	[id_justificativo] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[id_usuario_auditor] [int] NULL,
	[tipo_inasistencia] [varchar](100) NULL,
	[ruta_archivo] [varchar](500) NULL,
	[nota_adicional] [varchar](max) NULL,
	[fecha_carga] [datetime] NOT NULL,
	[estado] [varchar](50) NULL,
	[fecha_inasistencia_inicio] [datetime] NULL,
	[fecha_inasistencia_fin] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_justificativo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[legajo] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[legajo](
	[id_legajo] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[id_tipo_doc] [int] NOT NULL,
	[id_usuario_auditor] [int] NULL,
	[ruta_archivo] [varchar](500) NULL,
	[fecha_carga] [datetime] NOT NULL,
	[fecha_vencimiento] [datetime] NULL,
	[estado] [varchar](50) NULL,
	[presentado_fisico] [bit] NULL,
	[comentario] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_legajo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[materias] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[materias](
	[id_materia] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](150) NOT NULL,
	[carrera] [varchar](150) NULL,
	[curso] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_materia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[mesa_examen] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mesa_examen](
	[id_examen] [int] NOT NULL,
	[id_docente] [int] NOT NULL,
 CONSTRAINT [PK_mesa_examen] PRIMARY KEY CLUSTERED 
(
	[id_examen] ASC,
	[id_docente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Pais] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pais](
	[id_pais] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_pais] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[programas_materia] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[programas_materia](
	[id_programa] [int] IDENTITY(1,1) NOT NULL,
	[id_docente] [int] NOT NULL,
	[id_materia] [int] NOT NULL,
	[condicion] [varchar](100) NULL,
	[objetivos_especificos] [varchar](max) NULL,
	[objetivos_generales] [varchar](max) NULL,
	[horas_semanales] [varchar](50) NULL,
	[horas_cuatrimestrales] [varchar](50) NULL,
	[evaluacion] [varchar](max) NULL,
	[criterios_evaluacion] [varchar](max) NULL,
	[estrategias_metodologicas] [varchar](max) NULL,
	[estrategias_acompanamiento_virtual_remoto] [varchar](max) NULL,
	[condicion_regular] [varchar](max) NULL,
	[condicion_promocional] [varchar](max) NULL,
	[condicion_libre] [varchar](max) NULL,
	[examenes_virtuales] [varchar](max) NULL,
	[formato_curricular] [varchar](100) NULL,
	[ciclo_lectivo] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_programa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Provincia] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Provincia](
	[id_provincia] [int] IDENTITY(1,1) NOT NULL,
	[id_pais] [int] NOT NULL,
	[nombre] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_provincia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[reconocimiento_saberes] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[reconocimiento_saberes](
	[id_solicitud] [int] IDENTITY(1,1) NOT NULL,
	[id_materia] [int] NOT NULL,
	[id_alumno] [int] NOT NULL,
	[id_docente] [int] NULL,
	[comentario] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_solicitud] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Roles] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[id_rol] [int] IDENTITY(1,1) NOT NULL,
	[rol] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_rol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[roles_tipos_documentos] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[roles_tipos_documentos](
	[id_roles_tipos_documentos] [int] IDENTITY(1,1) NOT NULL,
	[id_rol] [int] NOT NULL,
	[id_tipo_doc] [int] NOT NULL,
	[obligatorio] [bit] NOT NULL,
	[anual] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_roles_tipos_documentos] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[tipo_examen] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_examen](
	[id_tipo_examen] [int] IDENTITY(1,1) NOT NULL,
	[tipo_examen] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_tipo_examen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[tipo_titulo] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_titulo](
	[id_tipo_titulo] [int] IDENTITY(1,1) NOT NULL,
	[nombre_titulo] [varchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_tipo_titulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[tipos_documentos] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipos_documentos](
	[id_tipo_doc] [int] IDENTITY(1,1) NOT NULL,
	[nombre_documento] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_tipo_doc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuarios] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios](
	[id_usuario] [int] IDENTITY(1,1) NOT NULL,
	[email] [varchar](150) NOT NULL,
	[password_hash] [varchar](255) NOT NULL,
	[estado_usuario] [bit] NOT NULL,
	[token_recuperacion] [varchar](255) NULL,
	[expiracion_token] [datetime] NULL,
	[nombre] [varchar](100) NULL,
	[apellido] [varchar](100) NULL,
	[dni] [varchar](20) NULL,
	[telefono] [varchar](30) NULL,
	[telefono_emergencia] [varchar](30) NULL,
	[lugar_nacimiento] [varchar](150) NULL,
	[contacto_emergencia] [varchar](150) NULL,
	[direccion] [varchar](255) NULL,
	[id_provincia] [int] NULL,
	[fecha_nac] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuarios_roles] Fecha de script: 19/8/2026 18:39:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios_roles](
	[id_usuario_rol] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[id_rol] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_usuario_rol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Docentes] ADD  DEFAULT ((0)) FOR [director_suplente]
GO
ALTER TABLE [dbo].[legajo] ADD  DEFAULT ((0)) FOR [presentado_fisico]
GO
ALTER TABLE [dbo].[roles_tipos_documentos] ADD  DEFAULT ((0)) FOR [obligatorio]
GO
ALTER TABLE [dbo].[roles_tipos_documentos] ADD  DEFAULT ((0)) FOR [anual]
GO
ALTER TABLE [dbo].[Usuarios] ADD  DEFAULT ((1)) FOR [estado_usuario]
GO
ALTER TABLE [dbo].[alumno_materia]  WITH CHECK ADD  CONSTRAINT [FK_AlumnoMateria_Alumno] FOREIGN KEY([id_alumno])
REFERENCES [dbo].[Alumnos] ([id_alumno])
GO
ALTER TABLE [dbo].[alumno_materia] CHECK CONSTRAINT [FK_AlumnoMateria_Alumno]
GO
ALTER TABLE [dbo].[alumno_materia]  WITH CHECK ADD  CONSTRAINT [FK_AlumnoMateria_Comision] FOREIGN KEY([id_comision])
REFERENCES [dbo].[comision] ([id_comision])
GO
ALTER TABLE [dbo].[alumno_materia] CHECK CONSTRAINT [FK_AlumnoMateria_Comision]
GO
ALTER TABLE [dbo].[alumno_materia]  WITH CHECK ADD  CONSTRAINT [FK_AlumnoMateria_Materia] FOREIGN KEY([id_materia])
REFERENCES [dbo].[materias] ([id_materia])
GO
ALTER TABLE [dbo].[alumno_materia] CHECK CONSTRAINT [FK_AlumnoMateria_Materia]
GO
ALTER TABLE [dbo].[Alumnos]  WITH CHECK ADD  CONSTRAINT [FK_Alumnos_Usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[Alumnos] CHECK CONSTRAINT [FK_Alumnos_Usuario]
GO
ALTER TABLE [dbo].[contenidos]  WITH CHECK ADD  CONSTRAINT [FK_Contenidos_Programa] FOREIGN KEY([id_programa])
REFERENCES [dbo].[programas_materia] ([id_programa])
GO
ALTER TABLE [dbo].[contenidos] CHECK CONSTRAINT [FK_Contenidos_Programa]
GO
ALTER TABLE [dbo].[docente_materia]  WITH CHECK ADD  CONSTRAINT [FK_DocenteMateria_Comision] FOREIGN KEY([id_comision])
REFERENCES [dbo].[comision] ([id_comision])
GO
ALTER TABLE [dbo].[docente_materia] CHECK CONSTRAINT [FK_DocenteMateria_Comision]
GO
ALTER TABLE [dbo].[docente_materia]  WITH CHECK ADD  CONSTRAINT [FK_DocenteMateria_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
GO
ALTER TABLE [dbo].[docente_materia] CHECK CONSTRAINT [FK_DocenteMateria_Docente]
GO
ALTER TABLE [dbo].[docente_materia]  WITH CHECK ADD  CONSTRAINT [FK_DocenteMateria_Materia] FOREIGN KEY([id_materia])
REFERENCES [dbo].[materias] ([id_materia])
GO
ALTER TABLE [dbo].[docente_materia] CHECK CONSTRAINT [FK_DocenteMateria_Materia]
GO
ALTER TABLE [dbo].[docente_tipo_titulo]  WITH CHECK ADD  CONSTRAINT [FK_DocenteTipoTitulo_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
GO
ALTER TABLE [dbo].[docente_tipo_titulo] CHECK CONSTRAINT [FK_DocenteTipoTitulo_Docente]
GO
ALTER TABLE [dbo].[docente_tipo_titulo]  WITH CHECK ADD  CONSTRAINT [FK_DocenteTipoTitulo_TipoTitulo] FOREIGN KEY([id_tipo_titulo])
REFERENCES [dbo].[tipo_titulo] ([id_tipo_titulo])
GO
ALTER TABLE [dbo].[docente_tipo_titulo] CHECK CONSTRAINT [FK_DocenteTipoTitulo_TipoTitulo]
GO
ALTER TABLE [dbo].[Docentes]  WITH CHECK ADD  CONSTRAINT [FK_Docentes_Usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[Docentes] CHECK CONSTRAINT [FK_Docentes_Usuario]
GO
ALTER TABLE [dbo].[Examenes]  WITH CHECK ADD  CONSTRAINT [FK_Examenes_Comision] FOREIGN KEY([id_comision])
REFERENCES [dbo].[comision] ([id_comision])
GO
ALTER TABLE [dbo].[Examenes] CHECK CONSTRAINT [FK_Examenes_Comision]
GO
ALTER TABLE [dbo].[Examenes]  WITH CHECK ADD  CONSTRAINT [FK_Examenes_Materia] FOREIGN KEY([id_materia])
REFERENCES [dbo].[materias] ([id_materia])
GO
ALTER TABLE [dbo].[Examenes] CHECK CONSTRAINT [FK_Examenes_Materia]
GO
ALTER TABLE [dbo].[Examenes]  WITH CHECK ADD  CONSTRAINT [FK_Examenes_TipoExamen] FOREIGN KEY([id_tipo_examen])
REFERENCES [dbo].[tipo_examen] ([id_tipo_examen])
GO
ALTER TABLE [dbo].[Examenes] CHECK CONSTRAINT [FK_Examenes_TipoExamen]
GO
ALTER TABLE [dbo].[Justificativos]  WITH CHECK ADD  CONSTRAINT [FK_Justificativos_Auditor] FOREIGN KEY([id_usuario_auditor])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[Justificativos] CHECK CONSTRAINT [FK_Justificativos_Auditor]
GO
ALTER TABLE [dbo].[Justificativos]  WITH CHECK ADD  CONSTRAINT [FK_Justificativos_Usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[Justificativos] CHECK CONSTRAINT [FK_Justificativos_Usuario]
GO
ALTER TABLE [dbo].[legajo]  WITH CHECK ADD  CONSTRAINT [FK_Legajo_Auditor] FOREIGN KEY([id_usuario_auditor])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[legajo] CHECK CONSTRAINT [FK_Legajo_Auditor]
GO
ALTER TABLE [dbo].[legajo]  WITH CHECK ADD  CONSTRAINT [FK_Legajo_TipoDoc] FOREIGN KEY([id_tipo_doc])
REFERENCES [dbo].[tipos_documentos] ([id_tipo_doc])
GO
ALTER TABLE [dbo].[legajo] CHECK CONSTRAINT [FK_Legajo_TipoDoc]
GO
ALTER TABLE [dbo].[legajo]  WITH CHECK ADD  CONSTRAINT [FK_Legajo_Usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[legajo] CHECK CONSTRAINT [FK_Legajo_Usuario]
GO
ALTER TABLE [dbo].[mesa_examen]  WITH CHECK ADD  CONSTRAINT [FK_MesaExamen_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
GO
ALTER TABLE [dbo].[mesa_examen] CHECK CONSTRAINT [FK_MesaExamen_Docente]
GO
ALTER TABLE [dbo].[mesa_examen]  WITH CHECK ADD  CONSTRAINT [FK_MesaExamen_Examen] FOREIGN KEY([id_examen])
REFERENCES [dbo].[Examenes] ([id_examen])
GO
ALTER TABLE [dbo].[mesa_examen] CHECK CONSTRAINT [FK_MesaExamen_Examen]
GO
ALTER TABLE [dbo].[programas_materia]  WITH CHECK ADD  CONSTRAINT [FK_ProgramasMateria_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
GO
ALTER TABLE [dbo].[programas_materia] CHECK CONSTRAINT [FK_ProgramasMateria_Docente]
GO
ALTER TABLE [dbo].[programas_materia]  WITH CHECK ADD  CONSTRAINT [FK_ProgramasMateria_Materia] FOREIGN KEY([id_materia])
REFERENCES [dbo].[materias] ([id_materia])
GO
ALTER TABLE [dbo].[programas_materia] CHECK CONSTRAINT [FK_ProgramasMateria_Materia]
GO
ALTER TABLE [dbo].[Provincia]  WITH CHECK ADD  CONSTRAINT [FK_Provincia_Pais] FOREIGN KEY([id_pais])
REFERENCES [dbo].[Pais] ([id_pais])
GO
ALTER TABLE [dbo].[Provincia] CHECK CONSTRAINT [FK_Provincia_Pais]
GO
ALTER TABLE [dbo].[reconocimiento_saberes]  WITH CHECK ADD  CONSTRAINT [FK_Reconocimiento_Alumno] FOREIGN KEY([id_alumno])
REFERENCES [dbo].[Alumnos] ([id_alumno])
GO
ALTER TABLE [dbo].[reconocimiento_saberes] CHECK CONSTRAINT [FK_Reconocimiento_Alumno]
GO
ALTER TABLE [dbo].[reconocimiento_saberes]  WITH CHECK ADD  CONSTRAINT [FK_Reconocimiento_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
GO
ALTER TABLE [dbo].[reconocimiento_saberes] CHECK CONSTRAINT [FK_Reconocimiento_Docente]
GO
ALTER TABLE [dbo].[reconocimiento_saberes]  WITH CHECK ADD  CONSTRAINT [FK_Reconocimiento_Materia] FOREIGN KEY([id_materia])
REFERENCES [dbo].[materias] ([id_materia])
GO
ALTER TABLE [dbo].[reconocimiento_saberes] CHECK CONSTRAINT [FK_Reconocimiento_Materia]
GO
ALTER TABLE [dbo].[roles_tipos_documentos]  WITH CHECK ADD  CONSTRAINT [FK_RolesTiposDoc_Rol] FOREIGN KEY([id_rol])
REFERENCES [dbo].[Roles] ([id_rol])
GO
ALTER TABLE [dbo].[roles_tipos_documentos] CHECK CONSTRAINT [FK_RolesTiposDoc_Rol]
GO
ALTER TABLE [dbo].[roles_tipos_documentos]  WITH CHECK ADD  CONSTRAINT [FK_RolesTiposDoc_TipoDoc] FOREIGN KEY([id_tipo_doc])
REFERENCES [dbo].[tipos_documentos] ([id_tipo_doc])
GO
ALTER TABLE [dbo].[roles_tipos_documentos] CHECK CONSTRAINT [FK_RolesTiposDoc_TipoDoc]
GO
ALTER TABLE [dbo].[Usuarios]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Provincia] FOREIGN KEY([id_provincia])
REFERENCES [dbo].[Provincia] ([id_provincia])
GO
ALTER TABLE [dbo].[Usuarios] CHECK CONSTRAINT [FK_Usuarios_Provincia]
GO
ALTER TABLE [dbo].[Usuarios_roles]  WITH CHECK ADD  CONSTRAINT [FK_UsuariosRoles_Rol] FOREIGN KEY([id_rol])
REFERENCES [dbo].[Roles] ([id_rol])
GO
ALTER TABLE [dbo].[Usuarios_roles] CHECK CONSTRAINT [FK_UsuariosRoles_Rol]
GO
ALTER TABLE [dbo].[Usuarios_roles]  WITH CHECK ADD  CONSTRAINT [FK_UsuariosRoles_Usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[Usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[Usuarios_roles] CHECK CONSTRAINT [FK_UsuariosRoles_Usuario]
GO
USE [master]
GO
ALTER DATABASE [Autogestion_Docente] SET  READ_WRITE 
GO
