using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ReportesApp.ControlEntidades
{
    class EntornoControl : Controlador
    {
        public EntornoControl() : base() { }

        /// <summary>
        /// crea un entorno y lo retorna, setea el entorno en uso, o sea pone el campo desuso en cero
        /// </summary>
        public Entorno Crear(String entornoInfo)
        {
            Entorno entorno = new Entorno() { infoEntorno = entornoInfo, desuso = 0};
            return entorno;
        }
        /// <summary>
        /// crea un entorno y lo retorna
        /// </summary>
        public Entorno Crear(String entornoInfo, int desuso)
        {
            Entorno entorno = new Entorno() { infoEntorno = entornoInfo, desuso = desuso };
            return entorno;
        }

        /// <summary>
        /// Retorna una lista de entornos obtenidos de la BD. 
        /// Propiedad de solo lectura
        /// </summary>
        public List<Entorno> Entornos
        {
            get
            {
                List<Entorno> entornos = (from entorn in cnx.Entorno
                                                select entorn).ToList();
                return entornos;
            }
        }

        /// <summary>
        /// Adiciona a la BD un entorno dado
        /// </summary>
        /// <param name="administrador">entorno que se pretende adicionar a la BD</param>
        public void Adicionar(Entorno entorno)
        {
            try
            {
                cnx.Entorno.Add(entorno);
                cnx.SaveChanges();
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error adicionando el entorno en la BD: " + entorno.infoEntorno + ". " + msg.Message);
            }
        }


        /// <summary>
        /// Dado un identificador de entorno retorna, de existir en la BD, el entorno en cuestion.
        /// Retorna null de no existir el entorno en la BD.
        /// </summary>      
        public Entorno GetEntorno(int idEntorno)
        {
            Entorno entor = this.cnx.Entorno.Single(a => a.idEntorno == idEntorno);
            return entor;
        }

        /// <summary>
        /// Edita un entorno dado. Debe pasar un mismo entorno seleccionado previamente el cual mantenga su idEntorno
        /// </summary>
        /// <param name="entorno">entorno con las modificaciones hechas</param>
        public void Editar(Entorno entorno)
        {
            try
            {
                Entorno entorn = this.GetEntorno(entorno.idEntorno);
                if (entorn != null)
                {
                    cnx.Entry(entorn).CurrentValues.SetValues(entorno);
                    cnx.Entry(entorn).State = EntityState.Modified;
                    cnx.SaveChanges();
                }
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error en el proceso de edición del entorno: " + entorno.infoEntorno + ". " + msg.Message);
            }
        }
    }
}
