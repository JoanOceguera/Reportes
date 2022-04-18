using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportesApp;

namespace ReportesApp.ControlEntidades
{
    class ConsecutivoControl : Controlador
    {
        private int tope = 999;
        public ConsecutivoControl() : base() {}

        /// <summary>
        /// Retorna el valor del consecutivo ke se encuentra en la BD.
        /// De no existir ningun valor consecutivo en dicha BD entonces adiciona dicho valor setandolo en 0
        /// </summary> 
        public Consecutivo GetValorActualConsecutivo()
        {
            Consecutivo consec = (from consecutivo in cnx.Consecutivo
                                  select consecutivo).FirstOrDefault();
            if (consec == null)
            {
                consec = (new Consecutivo() { consecutivoSecuencia = 0 });
                this.Adicionar(consec);
            }
            return consec;
        }

        /// <summary>
        /// Retorna el consecutivo siguiente. No setea dicho valor en la BD
        /// </summary>
        /// <returns></returns>
        public Consecutivo GetNext()
        {
            int incremento = 1;
            Consecutivo conActual = this.GetValorActualConsecutivo();
            if (conActual.consecutivoSecuencia >= tope)
                incremento = conActual.consecutivoSecuencia * -1;
            return new Consecutivo() { consecutivoSecuencia = conActual.consecutivoSecuencia + incremento };
        }

        /// <summary>
        /// Retorna el consecutivo siguientev y actualiza dicho valor en la BD
        /// </summary>
        /// <returns></returns>
        public Consecutivo GetAndSetNext()
        {
            int incremento = 1;
            Consecutivo conActual = this.GetValorActualConsecutivo();
            if (conActual.consecutivoSecuencia >= tope)
                incremento = conActual.consecutivoSecuencia * -1;
            conActual.consecutivoSecuencia += incremento;
            cnx.SaveChanges();

            return conActual;
        }

                /// <summary>
        /// Adiciona a la BD un consecutivo dado
        /// </summary>
        /// <param name="consecutivo">consecutivo que se pretende adicionar a la BD</param>
        private void Adicionar(Consecutivo consecutivo)
        {
            try
            {
                cnx.Consecutivo.Add(consecutivo);
                cnx.SaveChanges();   
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error adicionando el consecutivo en la BD: " + consecutivo.consecutivoSecuencia + ". " + msg.Message);
            }
        }
    }
}
