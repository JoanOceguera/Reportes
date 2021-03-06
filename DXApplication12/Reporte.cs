//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReportesApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class Reporte
    {
        public int idReporte { get; set; }
        public Nullable<int> idEntorno { get; set; }
        public int idEquipo { get; set; }
        public int idProblemaPosible { get; set; }
        public Nullable<int> idAdministradorDefectando { get; set; }
        public string numero { get; set; }
        public System.DateTime fecha_hora { get; set; }
        public string observacion { get; set; }
        public string nombreCliente { get; set; }
        public string departamento { get; set; }
        public string estado { get; set; }
        public string nombrePC { get; set; }
        public Nullable<int> orden { get; set; }
        public string observacionTecnica { get; set; }
    
        public virtual Administrador Administrador { get; set; }
        public virtual Entorno Entorno { get; set; }
        public virtual Equipo Equipo { get; set; }
        public virtual ProblemaPosible ProblemaPosible { get; set; }
    }
}
