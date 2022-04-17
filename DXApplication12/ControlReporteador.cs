using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportesApp.ControlEntidades;
using System.Data.Objects.DataClasses;
using ReportesApp;
using ReporteApp;
using HMAC;
using System.Security.Cryptography;

namespace ReportesApp
{
    public class ControlReporteadorUsuario
    {
        int CANT_RECIENTES_MOSTRAR = 10;

        ReporteControl creporte;
        ReporteSolucionadoControl creporteSol;        
        EquipoControl cequipo;
        AdministradorControl cadministrador;
        EntornoControl centorno;
        ProblemaPosibleControl cproblemaPosible;
        AccesoControl cacceso;
        DateTime fechaUltimoRepoPendienteTraido = new DateTime(100,1,1);
        String literalCheckeoInicioNombrePC = "INF";
        String salt = "860407";


        public ControlReporteadorUsuario()
        {
            this.creporte = new ReporteControl();
            this.creporteSol = new ReporteSolucionadoControl();
            this.cequipo = new EquipoControl();
            this.cadministrador = new AdministradorControl();
            this.centorno = new EntornoControl();
            this.cproblemaPosible = new ProblemaPosibleControl();
            this.cacceso = new AccesoControl();
        }
        /// <summary>
        /// Adicina un reporte y retorna el numero que le fue asignado.
        /// </summary>
        public String AdicionarReporte(Equipo equipo, String nombreCliente, String departamento,
                                     ProblemaPosible problemaPosible)
        { 
            String numero = String.Empty;
            ReporteControl rp = new ReporteControl();
            try 
	        {
                numero = GeneradorNumeroRepo.GetNextNumero(); //se obtiene numero de reporte el siguiente sin updatear la BD	        
		        Reporte repo = rp.Crear(numero,equipo,DateTime.Now,nombreCliente,departamento,Estados.PendienteADefectar,problemaPosible);
                rp.Adicionar(repo);
                numero = GeneradorNumeroRepo.GetAndSetNextNumero();
	        }
	        catch (Exception){ throw ; }
            return numero;
        }

        /// <summary>
        /// Muestra un maximo de 10 ultimos reportes del usuario pasado en 'nombreCliente'
        /// </summary>
        public List<ReporteReciente> GetReportesRecientesDe(String nombreCliente)
        {
            List<Reporte> reportes = this.creporte.GetLastDelCliente(CANT_RECIENTES_MOSTRAR, nombreCliente);
            List<ReporteSolucionado> reportesSoluc = this.creporteSol.GetLastDelCliente(CANT_RECIENTES_MOSTRAR - reportes.Count, nombreCliente);

            List<ReporteReciente> recientes = new List<ReporteReciente>();
            ReporteReciente rrec;

            foreach (Reporte repo in reportes)
            {
                rrec = new ReporteReciente();
                rrec.Estado = repo.estado;
                rrec.FechaHora = repo.fecha_hora;
                rrec.NombreEquipo = repo.Equipo.nombre;
                rrec.Numero = repo.numero;
                rrec.Observacion = repo.observacion;
                rrec.PosibleProblema = repo.ProblemaPosible.problemaInfo;
                recientes.Add(rrec);
            }
            foreach (ReporteSolucionado rsol in reportesSoluc)
            {
                rrec = new ReporteReciente();
                rrec.Estado = Estados.Solucionado;
                rrec.FechaHora = rsol.fecha_hora;
                rrec.FechaHoraFin = rsol.fecha_horaFin;
                rrec.NombreEquipo = rsol.Equipo.nombre;
                rrec.Numero = rsol.numero;
                rrec.Observacion = rsol.observacion;
                rrec.PosibleProblema = rsol.ProblemaPosible.problemaInfo;
                recientes.Add(rrec);
            }
            return recientes;
        }

        public List<ReporteReciente> GetReportesTodosDe(String nombreCliente)
        {

            List<Reporte> reportes = this.creporte.GetReportesPorNombreCliente(nombreCliente);
            List<ReporteSolucionado> reportesSoluc = this.creporteSol.GetReportesSolucionadosPorNombreCliente(nombreCliente);

            List<ReporteReciente> recientes = new List<ReporteReciente>();
            ReporteReciente rrec;

            foreach (Reporte repo in reportes)
            {
                rrec = new ReporteReciente();
                rrec.Estado = repo.estado;
                rrec.FechaHora = repo.fecha_hora;
                rrec.NombreEquipo = repo.Equipo.nombre;
                rrec.Numero = repo.numero;
                rrec.Observacion = repo.observacion;
                rrec.PosibleProblema = repo.ProblemaPosible.problemaInfo;
                recientes.Add(rrec);
            }
            foreach (ReporteSolucionado rsol in reportesSoluc)
            {
                rrec = new ReporteReciente();
                rrec.Estado = Estados.Solucionado;
                rrec.FechaHora = rsol.fecha_hora;
                rrec.FechaHoraFin = rsol.fecha_horaFin;
                rrec.NombreEquipo = rsol.Equipo.nombre;
                rrec.Numero = rsol.numero;
                rrec.Observacion = rsol.observacion;
                rrec.PosibleProblema = rsol.ProblemaPosible.problemaInfo;
                recientes.Add(rrec);
            }
            return recientes;
        }
        //************Hasta aki complementan las tareas para el modulo del lado del cliente****

        /// <summary>
        /// Retorna una lista de Reportes pendientes
        /// </summary>
        /// <returns></returns>
        public List<Reporte> GetReportesPendientes()
        {
            var reportes = this.creporte.Reportes;
            return reportes;
        }
        public List<Reporte> GetReportesPendientesPosterioresA(DateTime fecha)
        {
            return this.creporte.GetReportesPosteriorA(fecha);
        }
        public List<Reporte> GetReportesPendientesNuevos()
        {
            List<Reporte> reportes = this.creporte.GetReportesPosteriorA(this.fechaUltimoRepoPendienteTraido);

            if(reportes.Count > 0)
                this.fechaUltimoRepoPendienteTraido = reportes.Max(repo => repo.fecha_hora);
            return reportes;
        }
        /// <summary>
        /// Retorna una lista de Reportes pendiente del usuario pasado en 'nombreCliente'
        /// </summary>
        public List<Reporte> GetReportesPendientesDe(String nombreCliente)
        {
            return this.creporte.GetReportesPorNombreCliente(nombreCliente);
        }

        public Reporte GetReportePorId(int idReporte)
        {
            return this.creporte.GetReporte(idReporte);
        }
        /// <summary>
        /// Cambia el estado a un reporte dado, si el estado pasa a ser Solucionado, entonces se 
        /// pasa el reporte a la tabla de ReporteSolucionado y se borra de la tabla de Reporte
        /// </summary>
        public void CambiarEstado(Reporte reporte, String estado, String solucionDelAdministrador)
        {
            if (estado == Estados.Solucionado)//si se solucionó, se pasa para la tabla de ReporteSolucionado
            {
                ReporteSolucionado repoSol = creporteSol.Crear(reporte, solucionDelAdministrador);
                try
                {
                    creporteSol.Adicionar(repoSol);
                    creporte.Borrar(reporte);
                }
                catch (Exception) { throw; }
            }
            else //si no se ha solucionado solo se edita el reporte con el nuevo estado 
            {
                reporte.estado = estado;
                creporte.Editar(reporte);
            }
        }

        /// <summary>
        /// Inserta un equipo dado su nombre y una lista con sus problemasPosibles.
        /// Retorna el equipo insertado
        /// </summary>
        /// <returns></returns>
        public Equipo InsertarEquipo(String nombreEquipo, List<ProblemaPosible> problemasPosibles)
        {
            Equipo equipo = this.cequipo.Crear(nombreEquipo);
            equipo = this.cequipo.AdicionarProblemasPosibles(equipo, problemasPosibles);

            return equipo;
        }
        public bool InsertarEquipo(Equipo equipo)
        {
            try
            {
                this.cequipo.Adicionar(equipo);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public List<Equipo> GetEquipos()
        {
            return cequipo.Equipos;
        }

        public List<ReporteSolucionado> GetReportesSolucionadosPorNombre(String nombreCliente)
        {
            List<ReporteSolucionado> reposSolucionados = creporteSol.GetReportesSolucionadosPorNombreCliente(nombreCliente);
            return reposSolucionados;
        }

        public List<ReporteSolucionado> GetReportesSolucionados()
        {
            List<ReporteSolucionado> reposSolucionados = creporteSol.ReportesSolucionados;
            return reposSolucionados;
        }
        public List<ReporteSolucionado> GetReportesSolucionadosApartirDe(DateTime fecha)
        {
            return creporteSol.ReportesSolucionadosApartirDe(fecha);
        }
        public List<ReporteSolucionado> GetReportesSolucionadosPorNombreAdministrador(String nombreAdministrador)
        {
            return creporteSol.GetReportesSolucionadosPorNombreAdministrador(nombreAdministrador);
        }
        public List<ReporteSolucionado> GetReportesSolucionadosCant(int cantidad)
        {
            List<ReporteSolucionado> reposSolucionados = creporteSol.GetReportesSolucionadosCant(cantidad);
            return reposSolucionados;
        }

        public void EditarEquipo(int idEquipo, String nombreNuevo, List<ProblemaPosible> problemasPosiblesNuevos)
        {
            Equipo equipo = cequipo.GetEquipo(idEquipo);
            equipo.nombre = nombreNuevo;            

        }
        public bool EditarEquipo(Equipo equipo)
        {
            try
            {
                this.cequipo.Editar(equipo);
            }
            catch (Exception)
            {
                return false;                
            }
            return true;
        }
        /// <summary>
        /// Retorna una lista con todos los administradores desde la BD, retorna una lista vacia de no existir ningun administrador
        /// </summary>
        /// <returns></returns>
        public List<Administrador> GetAdministradores()
        {
            return this.cadministrador.Administradores;
        }

        public List<Administrador> GetAdministradoresEnUso()
        {
            return this.cadministrador.GetAdministradoresEnUso();
        }

        public void CambiarAdministradorYEstado(int idReporte, String nuevoEstado, int idAdministradorNuevo)
        {
            Reporte repoModificar = this.creporte.GetReporte(idReporte);
            Administrador admin = null;
                repoModificar.estado = nuevoEstado.ToString();
            if (repoModificar != null) //de existir el reporte en la BD
            {
                if (idAdministradorNuevo != -1)
                    admin = cadministrador.GetAdministrador(idAdministradorNuevo);
                else //borrar el administrador si tiene alguno establecido el reporte con idReporte pasado por parametro
                {
                    this.creporte.CambiarAdministrador(repoModificar, null);
                }

                if (idAdministradorNuevo != -1
                ) //de existir el administrador nuevo en la base de datos(este error no deberia ocurrir)
                    if (admin != null)
                        this.creporte.CambiarAdministrador(repoModificar, admin);
                    else
                        throw new Exception("No existe el administrador en la base de datos con id: " +
                                            idAdministradorNuevo);
            }
            else throw new CustomException("No existe un reporte en la base de datos con el id: " + idReporte.ToString());            
        }

        public void ConcluirReporte(int idReporte, String solucionAdministrador)
        { 
            Reporte repo = this.GetReportePorId(idReporte);
            if(repo == null) throw new Exception("No existe un reporte con el id: " + idReporte);//no debe suceder, solo por precaucion
            
            ReporteSolucionado repoSol = this.creporteSol.Crear(repo, solucionAdministrador);

            this.creporteSol.Adicionar(repoSol);
            this.creporte.Borrar(repo);            
        }

        public List<Entorno> GetEntornos()
        {
            return centorno.Entornos;
        }
        /// <summary>
        /// Adiciona un entorno a la BD, retorna el entorno adicionado. Si hubo algun error retorna null
        /// </summary>
        public Entorno AdicionarEntorno(String entornoInfo, int desuso)
        {
            Entorno entorn = null;
            try
            {
                entorn = this.centorno.Crear(entornoInfo, desuso);
                this.centorno.Adicionar(entorn);
            }
            catch (Exception)
            {
                return null;
            }
            return entorn;
        }
        /// <summary>
        /// Retorna un entorno dado su id, retorna null de no existir en la bd
        /// </summary>
        /// <returns></returns>
        public Entorno GetEntorno(int idEntorno)
        {
            return this.centorno.GetEntorno(idEntorno);
        }
        public void EditarEntorno(Entorno entorno)
        {
            this.centorno.Editar(entorno);
        }

        public Equipo GetEquipo(int idEquipo)
        {
            return this.cequipo.GetEquipo(idEquipo);
        }

        /// <summary>
        /// Retorna un administrador dado su id, retorna null de no existir en la BD
        /// </summary>
        public Administrador GetAdministrador(int idAdministrador)
        {
            return this.cadministrador.GetAdministrador(idAdministrador);
        }

        public void EditarAdministrador(Administrador administrador)
        {
            this.cadministrador.Editar(administrador);
        }

        /// <summary>
        /// Adiciona un administrador a la BD, retorna el administrador adicionado. Si hubo algun error retorna null
        /// </summary>
        public Administrador AdicionarAdministrador(String nombre, byte[] foto, int desuso, bool tecnico)
        {
            Administrador admin = null;
            try
            {
                admin = this.cadministrador.Crear(nombre, foto, desuso, tecnico);
                this.cadministrador.Adicionar(admin);
            }
            catch (Exception)
            {
                return null;
            }
            return admin;            
        }

        public ProblemaPosible CrearProblemaPosible(String problemaInfo, bool comsumible)
        {
            return this.cproblemaPosible.Crear(problemaInfo, comsumible);
        }
        public Equipo CrearEquipo(String nombreEquipo)
        {
            return this.cequipo.Crear(nombreEquipo);
        }

        /// <summary>
        /// Retorna true en caso de coincidir el comienzo del nombre de la PC con INF
        /// </summary>        
        public bool CheckearAnclaje()
        {
            String nombrePC = Environment.MachineName;
            if (nombrePC.Length >= this.literalCheckeoInicioNombrePC.Length)
                if (nombrePC.Substring(0, this.literalCheckeoInicioNombrePC.Length).ToUpper() == this.literalCheckeoInicioNombrePC.ToUpper())//si se corresponden los primeros caracteres del nombre de pc con el literal utilizado para comparar, en este caso INF
                    return true;
            return false;
        }

        public Acceso AdicionarAcceso(String pass)
        {
            Acceso acceso = null;
            try
            {
                HMACAlgorithm hmac = new HMACAlgorithm();
                hmac.Hash = new SHA1Managed(); //se especifica el hash a utilizar
                string hmacout = HMACAlgorithm.ToHex(hmac.HMAC(pass, salt));

                acceso = this.cacceso.Crear(hmacout);
                this.cacceso.AdicionarAcceso(acceso);
            }
            catch (Exception)
            {
                return null;
            }
            return acceso;
        }
        public bool ContraseñaCorrecta(String pass)
        { 
            HMACAlgorithm hmac = new HMACAlgorithm();
            hmac.Hash = new SHA1Managed(); //se especifica el hash a utilizar
            
            string hmacout = HMACAlgorithm.ToHex(hmac.HMAC(pass, salt));

            Acceso acc = this.cacceso.GetAccesoPorPass(hmacout);

            return (acc != null);
        }
        public bool HayAccesosEnBD()
        {
            List<Acceso> accessos = this.cacceso.Accesos;
            if (accessos != null && accessos.Count > 0)
                return true;
            return false;
        }
        public List<AdministradorCantSolucionados> GetRankingAdministradores()
        {
            return this.creporteSol.GetRankingAdministradores();
        }
        public List<AdministradorCantSolucionados> GetRankingAdministradoresApartirDe(DateTime fecha)
        {
            return this.creporteSol.GetRankingAdministradoresApartirDe(fecha);
        }
    }
}
