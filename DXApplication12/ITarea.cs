using System;
using System.Collections.Generic;
using System.Text;

namespace ReportesApp
{
    /// <summary>
    /// Interface Tarea, la cual debe implementar cada tipo de tarea ke 
    /// se necesite ke ejecute el temporizador
    /// </summary>
     public interface ITarea
    {
        void EjecutarTarea();
    }
}
