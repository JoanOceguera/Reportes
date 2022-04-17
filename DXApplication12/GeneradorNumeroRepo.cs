using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportesApp.ControlEntidades;
using System.Globalization;
using ReportesApp;

namespace ReporteApp
{
    static class GeneradorNumeroRepo
    {
        /// <summary>
        /// Retorna el siguiente numero de reporte. No setea dicho valor en la BD
        /// </summary>
        /// <returns></returns>
        public static String GetNextNumero()
        {
            ConsecutivoControl ccontrol = new ConsecutivoControl();
            Consecutivo cons = ccontrol.GetNext();
            String dtn = DateTime.Now.ToString("MMyy");
            String numero = cons.consecutivoSecuencia.ToString() + dtn.Substring(0,2) + dtn.Substring(2,2);

            return numero;
        }

        /// <summary>
        /// Retorna el siguiente numero de reporte. Setea dicho valor en la BD
        /// </summary>
        /// <returns></returns>
        public static String GetAndSetNextNumero()
        {
            ConsecutivoControl ccontrol = new ConsecutivoControl();
            Consecutivo cons = ccontrol.GetAndSetNext();
            String dtn = DateTime.Now.ToString("MMyy");
            String numero = cons.consecutivoSecuencia.ToString() + dtn.Substring(0, 2) + dtn.Substring(2, 2);

            return numero;
        }
    }
}
