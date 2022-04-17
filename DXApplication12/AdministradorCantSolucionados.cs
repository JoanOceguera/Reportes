using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportesApp
{
    public class AdministradorCantSolucionados
    {
        public int CantSolucionados { get; set; }
        public Administrador Administrador { get; set; }

        public AdministradorCantSolucionados(Administrador administrador, int cantSolucionados)
        {
            this.CantSolucionados = cantSolucionados;
            this.Administrador = administrador;
        }
        public AdministradorCantSolucionados()
        { }
    }
}
