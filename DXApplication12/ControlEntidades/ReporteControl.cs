using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ReportesApp;

namespace ReportesApp.ControlEntidades
{
    /// <summary>
    /// Control de los reportes ke no se han completado
    /// </summary>
    class ReporteControl : Controlador
    {
        public ReporteControl() : base() { }

        /// <summary>
        /// Crea y retorna un Reporte
        /// </summary>
        public Reporte Crear(String numero, Equipo equipo, DateTime fechaHora, String nombreCliente, String departamento, 
                              String estado, String observacion, ProblemaPosible problemaPosible, Administrador administradorDefectando )
        {
            Reporte reporte = new Reporte() { numero = numero, Equipo = equipo, fecha_hora = fechaHora, nombreCliente = nombreCliente, Administrador = administradorDefectando,
                                              departamento = departamento, estado = estado, observacion = observacion, ProblemaPosible = problemaPosible };
            return reporte;
        }
        /// <summary>
        /// Crea y retorna un Reporte sin haberle seteado el Administrador
        /// </summary>
        public Reporte Crear(String numero, Equipo equipo, DateTime fechaHora, String nombreCliente, String departamento,
                              String estado, String observacion, ProblemaPosible problemaPosible)
        {
            Reporte reporte = new Reporte()
            {
                numero = numero,
                Equipo = equipo,
                fecha_hora = fechaHora,
                nombreCliente = nombreCliente,
                departamento = departamento,
                estado = estado,
                observacion = observacion,
                ProblemaPosible = problemaPosible
            };
            return reporte;
        }
        /// <summary>
        /// Crea y retorna un Reporte sin haberle seteado el campo 'observacion'
        /// </summary>
        public Reporte Crear(String numero, Equipo equipo, DateTime fechaHora, String nombreCliente, String departamento,
                              String estado, ProblemaPosible problemaPosible, Administrador administradorDefectando)
        {
            Reporte reporte = new Reporte()
            {
                numero = numero,
                Equipo = equipo,
                fecha_hora = fechaHora,
                nombreCliente = nombreCliente,
                departamento = departamento,
                estado = estado,
                Administrador = administradorDefectando,
                ProblemaPosible = problemaPosible
            };
            return reporte;
        }
        /// <summary>
        /// Crea y retorna un Reporte sin haberle seteado los campos 'observacion', 'Administrador' (se refiere al administradorDefectando)
        /// </summary>
        public Reporte Crear(String numero, Equipo equipo, DateTime fechaHora, String nombreCliente, String departamento,
                              String estado, ProblemaPosible problemaPosible)
        {
            Reporte reporte = new Reporte()
            {
                numero = numero,
                Equipo = equipo,
                fecha_hora = fechaHora,
                nombreCliente = nombreCliente,
                departamento = departamento,
                estado = estado,
                ProblemaPosible = problemaPosible
            };
            return reporte;
        }

        /// <summary>
        /// Retorna una lista ordenada por fecha descendientemente (las mas recientes en las ultimas posiciones en la lista) 
        /// de reportes ke no se han completado, obtenidos de la BD. 
        /// Propiedad de solo lectura
        /// </summary>
        public List<Reporte> Reportes
        {
            get
            {
                List<Reporte> reportes = (from report in cnx.Reporte
                                          orderby report.fecha_hora ascending
                                          select report).ToList();                
                return reportes;
            }
        }

        /// <summary>
        /// Dado un identificador de reporte retorna, de existir en la BD, el reporte en cuestion.
        /// Retorna null de no existir el reporte en la BD.
        /// </summary>
        /// <param name="idEquipo">id del reporte que se desea seleccionar de la BD</param>        
        public Reporte GetReporte(int idReporte)
        {
            Reporte reporte = (from report in cnx.Reporte
                            where report.idReporte == idReporte
                            select report).FirstOrDefault();
            return reporte;
        }

        /// <summary>
        /// Dado un numero de reporte retorna, de existir en la BD, el reporte en cuestion.
        /// Retorna null de no existir el reporte en la BD.
        /// </summary>
        /// <param name="idEquipo">id del equipo que se desea seleccionar de la BD</param>        
        public Reporte GetReportePorNumero(String numero)
        {
            Reporte reporte = (from report in cnx.Reporte
                               where report.numero == numero
                               select report).FirstOrDefault();
            return reporte;
        }

        /// <summary>
        /// Retorna todos los reportes que pertenecen al cliente 'nombreCliente' pasado por parametro
        /// </summary>
        /// <param name="nombreCliente"></param>
        /// <returns></returns>
        public List<Reporte> GetReportesPorNombreCliente(String nombreCliente)
        {
            List<Reporte> reportes = (from report in cnx.Reporte
                                     where report.nombreCliente.ToUpper() == nombreCliente.ToUpper()
                                     select report).ToList();
            return reportes;
        }

        /// <summary>
        /// Retorna una lista de reportes que esten pendientes por Defectar. 
        /// De no haber ninguno en este estado, retorna la lista vacia
        /// </summary>
        /// <returns></returns>
        public List<Reporte> GetReportesPendienteADefectar()
        {
            return this.GetReportesPorEstado(Estados.PendienteADefectar);
        }

        /// <summary>
        /// Retorna una lista de reportes que esten siendo defectados. 
        /// De no haber ninguno en este estado, retorna la lista vacia
        /// </summary>
        /// <returns></returns>
        public List<Reporte> GetReportesSiendoDefectado()
        {
            return this.GetReportesPorEstado(Estados.SiendoDefectado);
        }

        public List<Reporte> GetReportesPorEstado(String estado)
        {
            List<Reporte> reportes = (from report in cnx.Reporte
                                      where report.estado.ToLower() == estado.ToLower()
                                      select report).ToList();
            return reportes;
        }

        /// <summary>
        /// Obtiene una lista de los repores con fecha mayor a fecha pasada por parametro.
        /// </summary>        
        public List<Reporte> GetReportesPosteriorA(DateTime fecha)
        {
            List<Reporte> reportes = cnx.Reporte.Where(x => x.fecha_hora > fecha).OrderBy(x => x.fecha_hora).ToList();
            List<Reporte> reportesUsuario = new List<Reporte>();
            foreach (var item in reportes)
            {
                reportesUsuario.Add(item);
            }
            return reportesUsuario;
        }

        /// <summary>
        /// Adiciona a la BD un reporte dado
        /// </summary>
        /// <param name="reporte">reporte que se pretende adicionar a la BD</param>
        public void Adicionar(Reporte reporte)
        {
            try
            {
                cnx.Reporte.AddObject(reporte);
                cnx.SaveChanges();
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error adicionando el reporte en la BD: " + reporte.numero + ". " + msg.Message);
            }
        }
        /// <summary>
        /// Setea en la BD un reporte dado que debe haber sido editado en el contexto. Debe pasar un mismo reporte seleccionado previamente el cual mantenga su idReporte
        /// </summary>
        /// <param name="reporte">reporte con las modificaciones hechas</param>
        public void Editar(Reporte reporte)
        {
            try
            {
                Reporte report = this.GetReporte(reporte.idReporte);
                if (report != null)
                {
                    cnx.Reporte.ApplyCurrentValues(report);
                    cnx.SaveChanges();
                }
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error en el proceso de edición del reporte: " + reporte.numero + ". " + msg.Message);
            }
        }

        /// <summary>
        /// Cambia el administrador al reporte pasado por parametro. Si se pasa null en administrador, entonces
        /// borra dicha relacion de la base de datos.
        /// </summary>
        public void CambiarAdministrador(Reporte reporteAModificar, Administrador administrador)
        {
            if (administrador == null)
                reporteAModificar.Administrador = null;
            reporteAModificar.Administrador = administrador;
            cnx.SaveChanges();
        }

        /// <summary>
        /// Retorna la ultima cantidad de reportes que se pase por parametro en 'cantidad' de la persona 'nombreCliente', ordenados por fecha.
        /// </summary>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public List<Reporte> GetLastDelCliente(int cantidad, String nombreCliente)
        {
            List<Reporte> reportes = (from report in cnx.Reporte
                                      orderby report.fecha_hora descending
                                      where report.nombreCliente.ToUpper() == nombreCliente.ToUpper()
                                      select report).Take(cantidad).ToList();
            return reportes;                           
        }

        /// <summary>
        /// Retorna la ultima cantidad de reportes que se pase por parametro en 'cantidad', ordenados por fecha.
        /// </summary>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public List<Reporte> GetLast(int cantidad)
        {
            List<Reporte> reportes = (from report in cnx.Reporte
                                      orderby report.fecha_hora descending
                                      select report).Take(cantidad).ToList();
            return reportes;
        }

        /// <summary>
        /// Borra un reporte de la tabla de Reporte de la base de datos
        /// </summary>
        public void Borrar(Reporte reporte)
        {
            try
            {
                Reporte rborrar = this.GetReporte(reporte.idReporte);
                this.cnx.DeleteObject(rborrar);                
                this.cnx.SaveChanges();
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error borrando el reporte: " + reporte.numero + ". " + msg.Message);
            }
        }

        public string QuitarAcentos(string inputString)
        {
            Regex a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            Regex n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
            inputString = a.Replace(inputString, "a");
            inputString = e.Replace(inputString, "e");
            inputString = i.Replace(inputString, "i");
            inputString = o.Replace(inputString, "o");
            inputString = u.Replace(inputString, "u");
            inputString = n.Replace(inputString, "n");
            return inputString;
        }
    }
}
