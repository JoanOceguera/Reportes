using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportesApp
{
    public class TareaActualizarReposPendientes : ITarea
    {
        public delegate void FuncionAEjecutar_delegado();
        public FuncionAEjecutar_delegado funcionAEjecutar = null;
        public void EjecutarTarea()
        {
            if (this.funcionAEjecutar != null)
                this.funcionAEjecutar();
            else throw new Exception("Debe especificar una funcion a ejecutar. Utilice para tal fin la función miembro 'SetFuncionAEjecutar'");
        }
        public void SetFuncionAEjecutar(FuncionAEjecutar_delegado funcionEjecutar)
        {
            this.funcionAEjecutar = funcionEjecutar;
        }
        public FuncionAEjecutar_delegado GetFuncionAEjecutar()
        {
            return this.funcionAEjecutar;
        }
    }
}
