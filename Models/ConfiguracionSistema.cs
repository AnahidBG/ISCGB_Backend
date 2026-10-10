using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoGestionAPI.Models
{
    [Table("configuracion_sistema")]
    public class ConfiguracionSistema
    {
        [Key]
        [Column("id_configuracion")]
        public int IdConfiguracion { get; set; }

        [Column("frecuencia_notificacion_dias")]
        public int? FrecuenciaNotificacionDias { get; set; }

        public int? LimiteParcialesDiario { get; set; }


    }
}