using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading;


namespace ReportesApp
{
    public class Temporizador
    {
        private ITarea tarea;
        private System.Timers.Timer timer;
        private int intervalo = 0;
        private int sincronizador = 0; //sincroniza el trabajo entre procesos, sucede ke puede comenzarce a ejecutar
                                       //en un subproceso el metodo EjecutarTarea antes de haber terminado con la
                                       //ejecucion del mismo en una instancia anterior. Esta variable maneja eso.

        public Temporizador(int intervalo)
        {
            this.Init(intervalo);
        }
        public Temporizador(int intervalo, ITarea tarea)
        {
            this.tarea = tarea;
            this.Init(intervalo);
        }
        private void Init(int intervalo)
        {
            this.intervalo = intervalo;
            this.timer = new System.Timers.Timer();
            this.timer.Interval = this.intervalo;
            this.timer.Elapsed += new ElapsedEventHandler(EjecutarTarea);
            this.IniciarTimer();
        }

        private void EjecutarTarea(object from, ElapsedEventArgs args)
        {                     
            int valorOriginalsincronizador = Interlocked.CompareExchange(ref sincronizador, 1, 0);
            if (valorOriginalsincronizador == 0)
            {                
                this.tarea.EjecutarTarea();
                Interlocked.CompareExchange(ref sincronizador, 0, 1);
            }            
        }

        public void SetTarea(ITarea tarea)
        {
            this.tarea = tarea;
        }
        public void PararTimer()
        {
            this.timer.Stop();
        }
        public void IniciarTimer()
        {           
            this.timer.Start();
        }
    }
}
