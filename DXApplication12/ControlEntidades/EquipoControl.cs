using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using ReportesApp;

namespace ReportesApp.ControlEntidades
{
    
    class EquipoControl : Controlador
    {

        public EquipoControl() : base() { }

        /// <summary>
        /// Retorna un Equipo seteando por defecto desuso en 0, lo cual significa que el equipo está activo.
        /// No tiene ProblemaPosible seteado, ni entorno definido.
        /// </summary>
        /// <param name="nombreEquipo">Nombre que se desea tenga el equipo</param>
        /// <returns></returns>
        public Equipo Crear(String nombreEquipo)
        {
            Equipo equip = new Equipo() { nombre = nombreEquipo, desuso = 0 };            
            return equip;              
        }

        /// <summary>
        /// Adiciona un ProblemaPosible a un equipo dado. Retorna el equipo insertado    
        /// </summary>
        public Equipo AdicionarProblemaPosible(Equipo equipo, ProblemaPosible problema)
        {
            equipo.ProblemaPosible.Add(problema);
            cnx.SaveChanges();

            return equipo;
        }

        /// <summary>
        /// Adiciona una lista de ProblemasPosibles a un equipo dado. Retorna el equipo insertado         
        /// </summary>
        public Equipo AdicionarProblemasPosibles(Equipo equipo, List<ProblemaPosible> problemasPosibles)
        {
            equipo.ProblemaPosible.Concat(problemasPosibles);
            cnx.SaveChanges();

            return equipo;
        }

        /// <summary>
        /// Retorna una lista de equipos obtenidos de la BD. 
        /// Propiedad de solo lectura
        /// </summary>
        public List<Equipo> Equipos
        {
            get{
                List<Equipo> equipos = (from equipo in cnx.Equipo
                                        select equipo).ToList();
                return equipos;
            }
        }
        /// <summary>
        /// Dado un identificador de equipo retorna, de existir en la BD, el equipo en cuestion.
        /// Retorna null de no existir el equipo en la BD.
        /// </summary>
        /// <param name="idEquipo">id del equipo que se desea seleccionar de la BD</param>        
        public Equipo GetEquipo(int idEquipo)
        {
            Equipo equip =  (from equipo in cnx.Equipo
                             where equipo.idEquipo == idEquipo
                             select equipo).FirstOrDefault();
            return equip;
        }

        public List<Equipo> GetEquiposEnUso()
        {
            List<Equipo> equipos = (from equipo in cnx.Equipo
                                    where equipo.desuso == 0
                                    select equipo).ToList();
            return equipos;
        }
        public List<Equipo> GetEquiposEnDesUso()
        {
            List<Equipo> equipos = (from equipo in cnx.Equipo
                                    where equipo.desuso == 1
                                    select equipo).ToList();
            return equipos;
        }
        /// <summary>
        /// Adiciona a la BD un equipo dado
        /// </summary>
        /// <param name="equipo">equipo que se pretende adicionar a la BD</param>
        public void Adicionar(Equipo equipo)
        {
            try
            {
                cnx.Equipo.Add(equipo);
                cnx.SaveChanges();   
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error adicionando el equipo en la BD: " + equipo.nombre + ". " + msg.Message);
            }
        }
        /// <summary>
        /// Edita un equipo dado. Debe pasar un mismo equipo seleccionado previamente el cual mantenga su idEquipo
        /// </summary>
        /// <param name="equipo">equipo con las modificaciones hechas</param>
        public void Editar(Equipo equipo)
        {
            try
            {
                Equipo equip = this.GetEquipo(equipo.idEquipo);
                if (equip != null)
                {
                    equip = equipo;
                    //cnx.Equipo.ApplyCurrentValues(equip);
                    cnx.SaveChanges();
                }
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error en el proceso de edición del equipo: " + equipo.nombre + ". " + msg.Message);
            }
        }

        /// <summary>
        /// No borra un equipo de la base de datos. Solamente setea el campo 'desuso' en 1
        /// </summary>
        /// <param name="idEquipo"></param>
        public void BorrarPorId(int idEquipo)
        {
            try
            {
                Equipo equip = this.GetEquipo(idEquipo);
                if (equip != null)
                {
                    equip.desuso = 1;
                    cnx.SaveChanges();
                }
                else throw new Exception("El equipo que intenta borrar no existe en la base de datos");
            }
            catch (Exception msg)
            {                
                throw new Exception("Ocurrió un error en el proceso de borrado del equipo. " + msg.Message);
            }
        }
        /// <summary>
        /// No borra un equipo de la base de datos. Solamente setea el campo 'desuso' en 1
        /// </summary>
        /// <param name="idEquipo"></param>
        public void Borrar(Equipo equipo)
        {
            try
            {
                Equipo equip = this.GetEquipo(equipo.idEquipo);
                if (equip != null)
                {
                    equip.desuso = 1;
                    cnx.SaveChanges();
                }
                else throw new Exception("El equipo que intenta borrar no existe en la base de datos");
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error en el proceso de borrado del equipo. " + msg.Message);
            }
        }

        /// <summary>
        /// Retorna true de no existir el equipo en la base de datos o estar marcado como desuso, esto es si el campo
        /// 'desuso' esta seteado en 1.
        /// </summary>
        public bool EstaBorradoPorId(int idEquipo)
        {
            Equipo equip = this.GetEquipo(idEquipo);
            if (equip != null)            
                return (equip.desuso == 1);
            
            return true;
        }
        /// <summary>
        /// Retorna true de no existir el equipo en la base de datos o estar marcado como desuso, esto es si el campo
        /// 'desuso' esta seteado en 1.
        /// </summary>
        public bool EstaBorrado(Equipo equipo)
        {
            Equipo equip = this.GetEquipo(equipo.idEquipo);
            if (equip != null)
                return (equip.desuso == 1);

            return true;
        }
    }
}
