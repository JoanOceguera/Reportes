using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportesApp.ControlEntidades
{
    class AccesoControl : Controlador
    {
        public AccesoControl() : base(){}

        /// <summary>
        /// Retorna un Acceso
        /// </summary>
        /// <param name="nombreEquipo">Nombre que se desea tenga el equipo</param>
        /// <returns></returns>
        public Acceso Crear(String pass)
        {
            Acceso acces = new Acceso() { pass = pass };
            return acces;
        }       

        /// <summary>
        /// Adiciona un aceso. Retorna el acceso insertado    
        /// </summary>
        public void AdicionarAcceso(Acceso acceso)
        {
            try
            {
                cnx.Acceso.AddObject(acceso);            
                cnx.SaveChanges();
            }
            catch (Exception)
            {
                throw new Exception("Ocurrio un problema adicionando la contraseña.");
            }
        }

        /// <summary>
        /// Edita un acceso dado. Debe pasar un mismo administrador seleccionado previamente el cual mantenga su idAdministrador
        /// </summary>
        /// <param name="administrador">administrador con las modificaciones hechas</param>
        public void Editar(Acceso acceso)
        {
            try
            {
                Acceso acces = this.GetAcceso(acceso.idAcceso);
                if (acces != null)
                {
                    cnx.Acceso.ApplyCurrentValues(acces);
                    cnx.SaveChanges();
                }
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error en el proceso de edición de contraseña: " + msg.Message);
            }
        }

        /// <summary>
        /// Dado un identificador de acceso retorna, de existir en la BD, el acceso en cuestion.
        /// Retorna null de no existir el acceso en la BD.
        /// </summary>      
        public Acceso GetAcceso(int idAcceso)
        {
            Acceso acceso = this.cnx.Acceso.Single(a => a.idAcceso == idAcceso);
            return acceso;
        }

        /// <summary>
        /// Retorna un acceso dado el pass
        /// </summary>
        public Acceso GetAccesoPorPass(String pass)
        {
            Acceso acces = (from acc in cnx.Acceso
                            where acc.pass.ToUpper() == pass.ToUpper()
                          select acc).FirstOrDefault();
            return acces;
        }

        /// <summary>
        /// Retorna una lista de accesos obtenidos de la BD. 
        /// Propiedad de solo lectura
        /// </summary>
        public List<Acceso> Accesos
        {
            get
            {
                List<Acceso> acceso = (from acc in cnx.Acceso
                                       select acc).ToList();
                return acceso;
            }
        }
    }
}
