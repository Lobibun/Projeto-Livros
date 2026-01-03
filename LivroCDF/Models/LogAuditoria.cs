using System;
using System.ComponentModel.DataAnnotations;

namespace LivroCDF.Models
{
    public class LogAuditoria
    {
        public int Id { get; set; }

        [Display(Name = "Data/Hora")]
        public DateTime DataAcao { get; set; } = DateTime.Now;

        [Display(Name = "Usuário")]
        public string Usuario { get; set; } 

        [Display(Name = "Ação")]
        public string Acao { get; set; } 
        [Display(Name = "Detalhes")]
        public string Detalhes { get; set; }
    }
}
