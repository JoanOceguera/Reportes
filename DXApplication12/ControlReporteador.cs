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
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using System.Reflection;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.Drawing;

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
        private Application objApp;
        private _Workbook objBook;


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

        public void CambiarOrdenReporte (Reporte reporte)
        {
            this.creporte.Editar(reporte);
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

        public void GenerarExcelReportes()
        {
            try
            {
                this.objApp = (Microsoft.Office.Interop.Excel.Application)new Microsoft.Office.Interop.Excel.Application();
                this.objBook = this.objApp.Workbooks.Add(Missing.Value);
                Sheets worksheets = this.objBook.Worksheets;
                worksheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                _Worksheet item = (_Worksheet)worksheets.get_Item((1));
                int num = this.Cuerpo(item, 1, 2);
                this.FilaxArea(item, 1, num);
                Range rg = item.get_Range("A1", "J1");
                Range rgLD = item.get_Range("C1", "H1");
                rg.EntireColumn.AutoFit();
                rgLD.EntireColumn.NumberFormat = "0.00";

                item.Name = "Resumen de Reportes";
                this.objApp.Visible = true;
                this.objApp.UserControl = true;
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                MessageBox.Show(string.Concat("Error: ", exception.Message, " Line: ", exception.Source), "Error");
            }
        }



        private int Cuerpo(_Worksheet worksheet, int columna, int fila)
        {
            columna = 64 + columna;
            int num = columna;
            int num1 = fila;
            char chr = Convert.ToChar(num);
            string str = string.Concat(chr.ToString(), num1.ToString());
            Range range = worksheet.get_Range(str, str);
            range.Cells.Merge(Missing.Value);
            range.HorizontalAlignment = HorizontalAlignment.Center;
            range[Missing.Value] = "Número";
            chr = Convert.ToChar(num + 1);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range variable = worksheet.get_Range(str, str);
            variable.Cells.Merge(Missing.Value);
            variable.HorizontalAlignment = HorizontalAlignment.Center;
            variable[Missing.Value] = "Fecha";
            variable.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            chr = Convert.ToChar(num + 2);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range range1 = worksheet.get_Range(str, str);
            range1.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            range1[Missing.Value] = "Equipo";
            chr = Convert.ToChar(num + 3);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range variable1 = worksheet.get_Range(str, str);
            variable1.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            variable1[Missing.Value] = "Problema";
            chr = Convert.ToChar(num + 4);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range range2 = worksheet.get_Range(str, str);
            range2.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            range2.Cells.Merge(Missing.Value);
            range2.HorizontalAlignment = HorizontalAlignment.Center;
            range2[Missing.Value] = "Observación";
            chr = Convert.ToChar(num + 5);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range variable2 = worksheet.get_Range(str, str);
            variable2.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            variable2.Cells.Merge(Missing.Value);
            variable2.HorizontalAlignment = HorizontalAlignment.Center;
            variable2[Missing.Value] = "Técnico Defectando";
            chr = Convert.ToChar(num + 6);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range range3 = worksheet.get_Range(str, str);
            range3.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            range3.Cells.Merge(Missing.Value);
            range3.HorizontalAlignment = HorizontalAlignment.Center;
            range3[Missing.Value] = "Estado";
            chr = Convert.ToChar(num + 7);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range variable3 = worksheet.get_Range(str, str);
            variable3.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            variable3.Cells.Merge(Missing.Value);
            variable3.HorizontalAlignment = HorizontalAlignment.Center;
            variable3[Missing.Value] = "Cliente";
            chr = Convert.ToChar(num + 8);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range range4 = worksheet.get_Range(str, str);
            range4.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            range4.Cells.Merge(Missing.Value);
            range4.HorizontalAlignment = HorizontalAlignment.Center;
            range4[Missing.Value] = "Área";
            chr = Convert.ToChar(num + 9);
            str = string.Concat(chr.ToString(), num1.ToString());
            Range variable4 = worksheet.get_Range(str, str);
            variable4.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            variable4.Cells.Merge(Missing.Value);
            variable4.HorizontalAlignment = HorizontalAlignment.Center;
            variable4[Missing.Value] = "PC";
            chr = Convert.ToChar(num);
            str = string.Concat(chr.ToString(), num1.ToString());
            chr = Convert.ToChar(num + 9);
            string str1 = string.Concat(chr.ToString(), num1.ToString());
            Range white = worksheet.get_Range(str, str);
            white.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
            white.Interior.Color = Color.White;
            return num1 + 1;
        }


        private int FilaxArea(_Worksheet worksheet, int columna, int fila)
        {
            int num;
            int num1 = fila;
            int num2 = 0;
            try
            {
                columna = 64 + columna;
                int num3 = columna;
                List<Reporte> list = (
                    from x in this.GetReportesPendientes()
                    orderby x.numero
                    select x).ToList<Reporte>();
                List<object[,]> objArrays = new List<object[,]>();
                foreach (Reporte reporte in list)
                {
                    string equipo = this.GetEquipo(reporte.idEquipo).nombre;
                    string problemaPosible = this.cproblemaPosible.GetProblemaPosible(reporte.idProblemaPosible).problemaInfo;
                    string administrador = "";
                    if (reporte.idAdministradorDefectando.HasValue)
                    {
                        administrador = this.GetAdministrador(reporte.idAdministradorDefectando.Value).nombre;
                    }
                    string str = "";
                    string str1 = reporte.estado;
                    if (str1 != null)
                    {
                        if (str1 != "d")
                        {
                            goto Label3;
                        }
                        str = "Defectando";
                        goto Label1;
                    }
                Label3:
                    str = "Pendiente";
                Label1:
                    object[,] shortDateString = new object[1, 10];
                    shortDateString[0, 0] = reporte.numero;
                    shortDateString[0, 1] = reporte.fecha_hora.Date.ToShortDateString();
                    shortDateString[0, 2] = equipo;
                    shortDateString[0, 3] = problemaPosible;
                    shortDateString[0, 4] = reporte.observacion;
                    shortDateString[0, 5] = administrador;
                    shortDateString[0, 6] = str;
                    shortDateString[0, 7] = reporte.nombreCliente;
                    shortDateString[0, 8] = reporte.departamento;
                    shortDateString[0, 9] = reporte.nombrePC;
                    objArrays.Add(shortDateString);
                }
                foreach (object[,] objArray in objArrays)
                {
                    char chr = Convert.ToChar(num3);
                    int num4 = num1 + num2;
                    string str2 = string.Concat(chr.ToString(), num4.ToString());
                    chr = Convert.ToChar(num3 + 9);
                    num4 = num1 + num2;
                    string str3 = string.Concat(chr.ToString(), num4.ToString());
                    Range range = worksheet.get_Range(str2, str3);
                    if (objArray != null)
                    {
                        object[,] item = objArrays[num2];
                        range[Missing.Value] = item;
                        range.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
                    }
                    num2++;
                }
                num = num1 + num2 + 1;
                return num;
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                MessageBox.Show(string.Concat("ERROR, Fila ", exception.Message));
            }
            num = num1 + num2 + 1;
            return num;
        }
    }
}
