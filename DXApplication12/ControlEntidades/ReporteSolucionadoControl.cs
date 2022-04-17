using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportesApp;

namespace ReportesApp.ControlEntidades
{
    class ReporteSolucionadoControl : Controlador
    {
        public ReporteSolucionadoControl() : base() { }

        /// <summary>
        /// Crea un ReporteSolucionado a partir de un Reporte que se encontraba en uno de los estados anteriores(sin concluir).
        /// Ademas se pasa la solucion que le dio el administrador.
        /// </summary>
        public ReporteSolucionado Crear(Reporte reporte, String solucion)
        {
            ReporteSolucionado reportSoluc = new ReporteSolucionado()
            {
                numero = reporte.numero,
                Equipo = reporte.Equipo,
                ProblemaPosible = reporte.ProblemaPosible,
                fecha_hora = reporte.fecha_hora, //fecha y hora de creado el reporte(sin concluir) inicialmente
                observacion = reporte.observacion,
                nombreCliente = reporte.nombreCliente,
                departamento = reporte.departamento,
                solucion = solucion,
                Administrador = reporte.Administrador,
                Entorno = reporte.Entorno
            };
            reportSoluc.fecha_horaFin = DateTime.Now;
            return reportSoluc;
        }

        /// <summary>
        /// Dado un identificador de reporteSolucionado retorna, de existir en la BD, el reporteSolucionado en cuestion.
        /// Retorna null de no existir dicho reporteSolucionado en la BD.
        /// </summary>
        /// <param name="idReporteSolucionado">id del reporteSolucoinado que se desea seleccionar de la BD</param>        
        public ReporteSolucionado GetReporteSolucionado(int idReporteSolucionado)
        {
            ReporteSolucionado reportSoluc = (from repSol in cnx.ReporteSolucionado
                                                where repSol.idEquipo == idReporteSolucionado
                                                select repSol).FirstOrDefault();
            return reportSoluc;
        }

        /// <summary>
        /// Retorna una lista ordenada por fecha ascendente
        /// de reportes ke no se han completado, obtenidos de la BD. Orden ascendente por fecha_horaFin
        /// Propiedad de solo lectura
        /// </summary>
        public List<ReporteSolucionado> ReportesSolucionados
        {
            get
            {
                List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                          orderby report.fecha_horaFin ascending
                                          select report).ToList();
                return reportes;
            }
        }

        /// <summary>
        /// Retorna una lista ordenada por fecha ascendente, dada la fecha a partir de la cual se desean obtener dichos reportes
        /// obtenidos de la BD. Orden ascendente por fecha_horaFin        
        /// </summary>
        public List<ReporteSolucionado> ReportesSolucionadosApartirDe(DateTime fecha)
        {
                List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                                     where report.fecha_horaFin >= fecha
                                                     orderby report.fecha_horaFin ascending
                                                     select report).ToList();
                return reportes;            
        }

        /// <summary>
        /// Retorna una cantidad de reportes(pasada por parametro dicha cantidad) mas recientes. Orden ascendente por fecha_horaFin
        /// </summary>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public List<ReporteSolucionado> GetReportesSolucionadosCant(int cantidad)
        {
            List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                                 orderby report.fecha_horaFin ascending                                                 
                                                 select report).Take(cantidad).ToList();
            return reportes;
        }

        /// <summary>
        /// Dado un numero de reporte retorna, de existir en la BD, el reporte en cuestion.
        /// Retorna null de no existir el reporte en la BD.
        /// </summary>
        /// <param name="idEquipo">id del equipo que se desea seleccionar de la BD</param>        
        public ReporteSolucionado GetReporteSolucionadoPorNumero(String numero)
        {
            ReporteSolucionado reporteSol = (from report in cnx.ReporteSolucionado
                                               where report.numero == numero
                                               select report).FirstOrDefault();
            return reporteSol;
        }

        /// <summary>
        /// Retorna todos los ReporteSolucionado que pertenecen al cliente 'nombreCliente' pasado por parametro
        /// </summary>
        /// <param name="nombreCliente"></param>
        /// <returns></returns>
        public List<ReporteSolucionado> GetReportesSolucionadosPorNombreCliente(String nombreCliente)
        {
            List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                      where report.nombreCliente.ToUpper() == nombreCliente.ToUpper()
                                      select report).OrderByDescending(x => x.fecha_horaFin).ToList();
            return reportes;
        }

        /// <summary>
        /// Retorna todos los ReporteSolucionado que pertenecen al administrador 'nombreAdministrador' pasado por parametro
        /// </summary>
        /// <param name="nombreAdministrador"></param>
        /// <returns></returns>
        public List<ReporteSolucionado> GetReportesSolucionadosPorNombreAdministrador(String nombreAdministrador)
        {
            List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                                 where report.Administrador.nombre.ToUpper().Contains(nombreAdministrador.ToUpper())
                                                 select report).OrderByDescending(x=> x.fecha_horaFin).ToList();
            return reportes;
        }

        /// <summary>
        /// Adiciona a la BD un reporteSolucionado dado. Si no se especifica fecha_horaFin, se setea dicho
        /// atributo con DateTime.Now
        /// </summary>
        /// <param name="reporteSolucionado">reporteSolucionado que se pretende adicionar a la BD</param>
        public void Adicionar(ReporteSolucionado reporteSolucionado)
        {
            try
            {
                if (this.GetReporteSolucionadoPorNumero(reporteSolucionado.numero) == null)//si no existe un reporteSolucionado con el mismo numero del ke se intenta adicionar
                {
                    if (reporteSolucionado.fecha_horaFin == null)
                        reporteSolucionado.fecha_horaFin = DateTime.Now;
                    cnx.ReporteSolucionado.AddObject(reporteSolucionado);
                    cnx.SaveChanges();
                }
                else //de existir un reporteSolucionado con el mismo numero ke el ke se intenta adicionar
                    throw new Exception("Existe un reporte solucionado que tiene el mismo número que el que se intenta adicionar. Numro: " + reporteSolucionado.numero);
            }
            catch (Exception msg)
            {
                throw new Exception("Ocurrió un error especificando en la BD que se ha solucionado el reporte: " + reporteSolucionado.numero + ". " + msg.Message);
            }
        }

        /// <summary>
        /// Retorna la ultima cantidad de reporteSolucionado que se pase por parametro en 'cantidad' de la persona 'nombreCliente', ordenados por fecha.
        /// </summary>
        /// <returns></returns>
        public List<ReporteSolucionado> GetLastDelCliente(int cantidad, String nombreCliente)
        {
            List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                      orderby report.fecha_hora descending
                                      where report.nombreCliente.ToUpper() == nombreCliente.ToUpper()
                                      select report).Take(cantidad).ToList();
            return reportes;
        }

        /// <summary>
        /// Retorna la ultima cantidad de ReporteSolucionado que se pase por parametro en 'cantidad', ordenados por fecha.
        /// </summary>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public List<ReporteSolucionado> GetLast(int cantidad)
        {
            List<ReporteSolucionado> reportes = (from report in cnx.ReporteSolucionado
                                      orderby report.fecha_hora descending
                                      select report).Take(cantidad).ToList();
            return reportes;
        }

        /// <summary>
        /// Retorna una lista de AdministradorCantSolucionados, este tipo contiene los miembros cantidad de reportes
        /// solucionados y el administrador asociado a dicha cantidad
        /// </summary>
        public List<AdministradorCantSolucionados> GetRankingAdministradores()
        {      
            try
            {
                List<AdministradorCantSolucionados> result = ReportesSolucionados.Where(x => !(bool)x.ProblemaPosible.consumible)
                    .GroupBy(idadmin => idadmin.idAdministradorDefecto)
                    .Select(admCantSol => new AdministradorCantSolucionados
                    {
                        Administrador = admCantSol.First().Administrador,
                        CantSolucionados = admCantSol.Count()
                    }).OrderBy(cant => cant.CantSolucionados).ToList();
                return result;
            }
            catch (Exception)
            {
                throw new Exception("Ocurrió un error confeccionando el reporte.");
            }
        }

        public List<AdministradorCantSolucionados> GetRankingAdministradoresApartirDe(DateTime fecha)
        {
            try
            {
                List<AdministradorCantSolucionados> result = ReportesSolucionados
                    .Where(repo => repo.fecha_horaFin >= fecha && !(bool)repo.ProblemaPosible.consumible)
                    .GroupBy(idadmin => idadmin.idAdministradorDefecto)                    
                    .Select(admCantSol => new AdministradorCantSolucionados
                    {
                        Administrador = admCantSol.First().Administrador,
                        CantSolucionados = admCantSol.Count()
                    }).OrderBy(cant => cant.CantSolucionados).ToList();
                return result;
            }
            catch (Exception)
            {
                throw new Exception("Ocurrió un error confeccionando el reporte.");
            }
        }
    }
}
