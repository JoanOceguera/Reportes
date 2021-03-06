using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ReportesApp.ControlEntidades;
using System.Threading;
using System.IO;
using System.Globalization;
using ReportesApp.ServiceReference1;

namespace ReportesApp
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private ReporteDBEntities _cnx;
        private String TEXT_SIN_ADMINISTRADOR = "Sin asignar";
        private String COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO = "Orange";
        private String COLOR_NOMBRE_ADMINISTRADOR_SINASIGNAR = "DimGray";
        private String COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO_SOLUCIONADOS = "DimGray";
        private String COLOR_REPORTEVISUAL_CON_ADMINISTRADOR = "LightGray";
        private String COLOR_REPORTEVISUAL_SIN_ADMINISTRADOR = "WhiteSmoke";
        private String COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR = "YellowGreen";
        private String COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_COMSUMIBLE = "RoyalBlue";
        private String COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR = "IndianRed";
        private String COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_COMSUMIBLE = "LightPink";
        private String COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_TEXTCOLOR = "DimGray";
        private String COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_TEXTCOLOR = "LightGray";

        private String EDITAR_TEXT = "editar";
        private String QUITAR_TEXT = "quitar";
        List<DevExpress.XtraEditors.PanelControl> panelesInfo;
        ControlReporteadorUsuario controlRepo;
        ProblemaPosibleControl controlProblema;
        EquipoControl controlEquipo;
        Entorno entornoSeleccionadoEditar;
        Administrador administradorSeleccionadoEditar;
        Equipo equipoSeleccionadoEditar;
        Dictionary<String, String> estadosText;
        public int CANT_REPORTES_SOLUCIONADOS = 150;

        public enum PanelCRUD { Editar = 2, Borrar = 1, Adicionar = 3 };
        public enum UsoEntidades { DesUso = 1, Uso = 0};

        Temporizador tempo;
        int INTERVALO_REFRESCO_REPOS_PENDIENTES = 300000;

        public Form1()
        {
            var reporteData = new ReporteDBEntities();
            Service1Client servicio = new Service1Client();
            controlProblema = new ProblemaPosibleControl();
            controlRepo = new ControlReporteadorUsuario();
            controlEquipo = new EquipoControl();
            ReportesApp.ControlEntidades.ReporteControl creporte = new ControlEntidades.ReporteControl();
            ReportesApp.ControlEntidades.AdministradorControl cadmin = new ControlEntidades.AdministradorControl();
            InitializeComponent();
            personalRHBindingSource.DataSource = servicio.DameTodosTrabajadores().OrderBy(x => x.Nombre).ToList();;
            equipoBindingSource.DataSource = controlEquipo.GetEquiposEnUso();
            estadosText = new Dictionary<String, String>();
            estadosText.Add(Estados.PendienteADefectar, "Pendiente");
            estadosText.Add(Estados.SiendoDefectado, "En proceso");
            estadosText.Add(Estados.Solucionado, "Solucionado");

            
            problemaPosibleBindingSource.DataSource = controlProblema.GetProblemasPosiblesEnUsoPorEquipo((int) comboEquipo.SelectedValue);

            this.panelesInfo = new List<DevExpress.XtraEditors.PanelControl>()
            {
                this.pnl_pendientes,
                this.pnl_solucionados,
                this.pnl_administrar
            };            
            
            this.PrepararReporteVisual();

            this.tempo = new Temporizador(INTERVALO_REFRESCO_REPOS_PENDIENTES);
            TareaActualizarReposPendientes tareaActRepos = new TareaActualizarReposPendientes();            
            tareaActRepos.SetFuncionAEjecutar(this.PrepararReporteVisual);
            this.tempo.SetTarea(tareaActRepos);
            this.tempo.IniciarTimer();
        }

        private void tileItem1_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            this.pnl_filtrar.Visible = false;
            this.TraerAlFrente(this.pnl_pendientes);
            this.PrepararReporteVisual();
            this.lbl_titulo.Text = "Reportes pendientes";
            this.ActualizarReporteVisualPeriodicamente();
        }
        private void tileItem2_ItemClick_1(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {            
            this.MostrarProgressCargando();            
            Application.DoEvents();
            if (!this.worker_mostrarSolucionados.IsBusy)
                this.worker_mostrarSolucionados.RunWorkerAsync();
            this.lbl_titulo.Text = "Reportes solucionados";
            this.pnl_filtrar.Visible = true;
        }
        private void tileItem3_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            (new MenuAdministracionReportes(this.controlRepo)).ShowDialog();
        }
        public void MostrarProgressCargando()
        {
            this.prog_cargandoSolucionados.Visible = true;
        }
        public void OcultarProgressCargando()
        {
            this.prog_cargandoSolucionados.Visible = false;            
        }
        private void TraerAlFrente(DevExpress.XtraEditors.PanelControl panel)
        {
            panel.BringToFront();
            panel.Visible = true; 

            foreach (var pane in this.panelesInfo)
            {
                if (pane.Name != panel.Name)
                {
                    pane.SendToBack();
                    pane.Visible = false;
                }
            }
        }

        delegate void PrepararReporteVisualSolucionados_delegado();
        public void PrepararReporteVisualSolucionados()
        {
            if (splitContainerControl2.InvokeRequired)
            {
                PrepararReporteVisualSolucionados_delegado d = new PrepararReporteVisualSolucionados_delegado(PrepararReporteVisualSolucionados);
                this.Invoke(d, new object[] { });
            }
            else
                {
                this.splitContainerControl2.Panel1.Controls.Clear();

                List<ReporteSolucionado> reposSolucionados;
                if(this.txt_nombreFilter.Text != String.Empty)
                    reposSolucionados = controlRepo.GetReportesSolucionadosPorNombreAdministrador(this.txt_nombreFilter.Text);
                else
                    reposSolucionados = /*controlRepo.GetReportesSolucionadosCant(CANT_REPORTES_SOLUCIONADOS)*/
                                          controlRepo.GetReportesSolucionadosApartirDe(DateTime.Today.AddMonths(-1));
                String cabecera;
                String descripcion;
                String entorno;
                String fechaHora;
                String nombreAdministrdor;
                String departamento;
                String observacion;
                //String solucion;

                ReporteSolucionadoVisual repSolDentroRepoControl;
                String color;

                foreach (ReporteSolucionado repo in reposSolucionados)
                {
                    if (repo.Administrador != null)
                    {
                        nombreAdministrdor = repo.Administrador.nombre;
                        color = COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO_SOLUCIONADOS;
                    }
                    else
                    {
                        nombreAdministrdor = String.Empty;
                        color = COLOR_NOMBRE_ADMINISTRADOR_SINASIGNAR;
                    }

                    repSolDentroRepoControl = new ReporteSolucionadoVisual(repo.idReporteSolucionado, repo.numero, repo.Equipo.nombre, repo.ProblemaPosible.problemaInfo,
                                                                           repo.fecha_hora, repo.fecha_horaFin,repo.nombreCliente, repo.solucion, repo.nombrePC, nombreAdministrdor, repo.departamento, repo.observacion);

                    if(repo.Entorno == null)
                        repSolDentroRepoControl.Entorno = String.Empty;
                    else repSolDentroRepoControl.Entorno = repo.Entorno.infoEntorno;
                    cabecera = repo.numero + "  " + repo.nombreCliente + " : " + repo.departamento;
                    descripcion = repo.Equipo.nombre + " : " + repo.ProblemaPosible.problemaInfo;
                    fechaHora = "Reportado: " + repo.fecha_hora.ToString("hh:mm tt dd/M", CultureInfo.InvariantCulture) + " Finalizado: " + repo.fecha_horaFin.ToString("hh:mm tt dd/M/yyyy", CultureInfo.InvariantCulture);
                    if(repo.Entorno == null)
                        entorno = String.Empty;
                    else entorno = repo.Entorno.infoEntorno;
                    // nombreAdministrdor = repo.Administrador.nombre;
                    departamento = repo.departamento;
                    observacion = repo.observacion;

                    WindowsFormsControlLibrary1.UserControlSol controlRepVis = this.CrearControlReporteVisualSolucionados(cabecera, descripcion, entorno, fechaHora, repSolDentroRepoControl, nombreAdministrdor);
                    controlRepVis.NombreAdminColor = color;

                    if (nombreAdministrdor != String.Empty && repo.Administrador.foto != null)
                    {
                        Image imgAdmin = ImagenConvert.byteArrayToImage(repo.Administrador.foto);
                        controlRepVis.ImagenAdministrador = imgAdmin;
                    }
                    this.splitContainerControl2.Panel1.Controls.Add(controlRepVis);
                }
            }
        } 


        public void PrepararReporteVisual()
        {
           // this.AdicionarReporteVisual(null);//limpia la lista de reportes
            List<Reporte> reposPendientes = controlRepo.GetReportesPendientesNuevos().OrderBy(x => x.orden).ToList();
            String cabecera;
            String descripcion;
            String entorno;
            String fechaHora;
            String nombreAdministrdor;
            String departamento;
            String observacion;
            ReporteVisual repDentroRepoControl;    
            String color;
            String colorFondo;
            String colorIndicadorIzq;

            foreach (Reporte repo in reposPendientes)
            {
                if(repo.Administrador != null)
                {
                    nombreAdministrdor = repo.Administrador.nombre;
                    color = COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO;
                    colorFondo = COLOR_REPORTEVISUAL_CON_ADMINISTRADOR;
                    if (repo.ProblemaPosible != null)
                    {
                        if ((bool) repo.ProblemaPosible.consumible)
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_COMSUMIBLE;
                        }
                        else
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR;
                        }
                        
                    }
                    else
                    {
                        colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR;
                    }
                    
                }
                else 
                {
                    nombreAdministrdor = String.Empty;
                    color = COLOR_NOMBRE_ADMINISTRADOR_SINASIGNAR;
                    colorFondo = COLOR_REPORTEVISUAL_SIN_ADMINISTRADOR;
                    if (repo.ProblemaPosible != null)
                    {
                        if ((bool)repo.ProblemaPosible.consumible)
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_COMSUMIBLE;
                        }
                        else
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR;
                        }

                    }
                    else
                    {
                        colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR;
                    }
                }
                if (repo.orden != null)
                {
                    repDentroRepoControl = new ReporteVisual(repo.idReporte, repo.numero, repo.Equipo.nombre, repo.ProblemaPosible.problemaInfo,
                    repo.fecha_hora, repo.nombreCliente, repo.estado, repo.nombrePC, nombreAdministrdor, repo.departamento, repo.observacion);
                }
                else
                {
                    repDentroRepoControl = new ReporteVisual(repo.idReporte, repo.numero, repo.Equipo.nombre, repo.ProblemaPosible.problemaInfo,
                    repo.fecha_hora, repo.nombreCliente, repo.estado, repo.nombrePC, nombreAdministrdor, repo.departamento, repo.observacion);
                }
                cabecera = repo.numero + "  " + repo.nombreCliente + " : " + repo.departamento;
                descripcion = repo.Equipo.nombre + " : " + repo.ProblemaPosible.problemaInfo;
                fechaHora = repo.fecha_hora.ToString("hh:mm tt dd/M/yyyy", CultureInfo.InvariantCulture);
                if(repo.Entorno == null)
                    entorno = String.Empty;
                else entorno = repo.Entorno.infoEntorno;
                departamento = repo.departamento;
                observacion = repo.observacion;               

                WindowsFormsControlLibrary1.UserControl1 controlRepVis = this.CrearControlReporteVisual(cabecera, descripcion, entorno, fechaHora, repDentroRepoControl, nombreAdministrdor);
                controlRepVis.NombreAdminColor = color;
                controlRepVis.SetColorNormalFondo(colorFondo);
                controlRepVis.SetColorIndicadorIzquierdo(colorIndicadorIzq);

                if (nombreAdministrdor != String.Empty && repo.Administrador.foto != null)
                {
                    if (repo.Administrador.foto != null)
                    {
                        Image imgAdmin = ImagenConvert.byteArrayToImage(repo.Administrador.foto);
                        controlRepVis.ImagenAdministrador = imgAdmin;
                    }
                }
                this.AdicionarReporteVisual(controlRepVis);                
            }
            this.ActualizarReporteVisualPeriodicamente();
        }

        delegate void AdicionarReporteVisual_delegado(WindowsFormsControlLibrary1.UserControl1 reporteVisualControl);
        /// <summary>
        /// si se pasa null entonces vacia el panel
        /// </summary>
        /// <param name="reporteVisualControl"></param>
        public void AdicionarReporteVisual(WindowsFormsControlLibrary1.UserControl1 reporteVisualControl)
        {
            if (this.splitContainerControl1.Panel1.InvokeRequired)
            {
                AdicionarReporteVisual_delegado d = new AdicionarReporteVisual_delegado(AdicionarReporteVisual);
                this.Invoke(d, new object[] { reporteVisualControl });
            }
            else
            {
                if (reporteVisualControl == null)
                    this.splitContainerControl1.Panel1.Controls.Clear();
                else
                    this.splitContainerControl1.Panel1.Controls.Add(reporteVisualControl);  
            }
        }

        public void MostrarInformacionReporte(ReporteVisual reporte)
        {
            this.LimpiarDatosInformacionReporte();
            this.LimpiarReporteSolucionesAnteriores();
            this.txt_numero.Text = reporte.Numero;
            this.txt_equipo.Text = reporte.NombreEquipo;
            this.txt_pcorigen.Text = reporte.NombrePC;
            this.txt_departamento.Text = reporte.Departamento;
            this.txt_observacion.Text = reporte.Observacion;
            this.txt_usuario.Text = reporte.NombreCliente;
            this.txt_administrador.Text = reporte.NombreAdministrador;
            this.txt_problema.Text = reporte.ProblemaPosibleText;
            this.txt_estado.Text = this.estadosText[reporte.Estado];// esta salida hay ke formatearla para ke no salga la leyenda textual sino algo mas pretty
            if (reporte.NombreAdministrador != String.Empty)
            {
                this.txt_estado.BackColor = Color.FromName(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR);
                this.txt_estado.ForeColor = Color.FromName(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_TEXTCOLOR);
            }
            else
            {
                this.txt_estado.BackColor = Color.FromName(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR);
                this.txt_estado.ForeColor = Color.FromName(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_TEXTCOLOR);
            }
            this.MostrarReportesSolucionesAnteriores(reporte.NombreCliente);
        }

        public void LimpiarDatosInformacionReporte()
        {
            this.txt_numero.Text = String.Empty;
            this.txt_equipo.Text = String.Empty;
            this.txt_pcorigen.Text = String.Empty;
            this.txt_departamento.Text = String.Empty;
            this.txt_observacion.Text = String.Empty;
            this.txt_usuario.Text = String.Empty;
            this.txt_administrador.Text = String.Empty;
            this.txt_problema.Text = String.Empty;
            this.txt_estado.Text = String.Empty;// esta salida hay ke formatearla para ke no salga la leyenda textual sino algo mas pretty
        }
        public void LimpiarReporteSolucionesAnteriores()
        {
            this.dtg_soluciones.Rows.Clear();
        }   
        public void MostrarReportesSolucionesAnteriores(String usuario)
        {
            List<ReporteSolucionado> reposSoluc = this.controlRepo.GetReportesSolucionadosPorNombre(usuario);
            String solucion = String.Empty;

            this.dtg_soluciones.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            for (int i = 0; i < reposSoluc.Count; i++)
			{
			    this.dtg_soluciones.Rows.Add();
                this.dtg_soluciones.Rows[i].Cells[0].Value = reposSoluc[i].Equipo.nombre;
                this.dtg_soluciones.Rows[i].Cells[1].Value = reposSoluc[i].ProblemaPosible.problemaInfo;
                solucion = reposSoluc[i].solucion;
                if (solucion.Length > 60)//si es muy largo el texto se divide para mejor visualizacion en el grid
                {
                    solucion = solucion.Substring(0, solucion.Length / 2) + Environment.NewLine + solucion.Substring(solucion.Length / 2);
                    this.dtg_soluciones.Rows[i].Height = 32;
                }
                this.dtg_soluciones.Rows[i].Cells[2].Value = solucion;
                this.dtg_soluciones.Rows[i].Cells[3].Value = reposSoluc[i].fecha_horaFin.ToString("dd/M/yyyy");
			}
        }

        public WindowsFormsControlLibrary1.UserControl1 CrearControlReporteVisual(String cabecera, String descripcion, String entorno,
                                                                                  String fechaHora, ReporteVisual reporte, String nombreAdministrador)
        {
            WindowsFormsControlLibrary1.UserControl1 reporteVisualControl = new WindowsFormsControlLibrary1.UserControl1();
            reporteVisualControl.CabeceraText = cabecera;
            reporteVisualControl.DescripcionText = descripcion;
            reporteVisualControl.Dock = System.Windows.Forms.DockStyle.Top;
            reporteVisualControl.Entorno = entorno;
            reporteVisualControl.FechaHora = fechaHora;
            reporteVisualControl.Location = new System.Drawing.Point(0, 0);
            reporteVisualControl.Reporte = reporte;

            reporteVisualControl.NombreAdministrador = (nombreAdministrador == String.Empty) ? this.TEXT_SIN_ADMINISTRADOR : nombreAdministrador;

            reporteVisualControl.pict_administrador.Click += new EventHandler(pict_administrador_Click);
            reporteVisualControl.pnl_administrador.Click += new EventHandler(pict_administrador_Click);
            reporteVisualControl.lbl_cabecera.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.lbl_descripcion.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.pnl_fondo.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.pnl_entorno.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.lbl_fechaHora.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.lbl_entorno.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.pnl_fechaHora.Click += new EventHandler(reportComponentClick);
            reporteVisualControl.btn_solucionarReporte.Click += new EventHandler(solucionadoClick);

            return reporteVisualControl;
        }

        public WindowsFormsControlLibrary1.UserControlSol CrearControlReporteVisualSolucionados(String cabecera, String descripcion, String entorno,
                                                                          String fechaHora, ReporteSolucionadoVisual reporte, String nombreAdministrador)
        {
            WindowsFormsControlLibrary1.UserControlSol reporteVisualControl = new WindowsFormsControlLibrary1.UserControlSol();
            reporteVisualControl.CabeceraText = cabecera;
            reporteVisualControl.DescripcionText = descripcion;
            reporteVisualControl.Dock = System.Windows.Forms.DockStyle.Top;
            reporteVisualControl.Entorno = entorno;
            reporteVisualControl.FechaHora = fechaHora;
            reporteVisualControl.Location = new System.Drawing.Point(0, 0);
            reporteVisualControl.Reporte = reporte;

            reporteVisualControl.NombreAdministrador = (nombreAdministrador == String.Empty) ? this.TEXT_SIN_ADMINISTRADOR : nombreAdministrador;


            reporteVisualControl.lbl_cabecera.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.lbl_descripcion.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.pnl_fondo.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.pnl_entorno.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.lbl_fechaHora.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.lbl_entorno.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.pnl_fechaHora.Click += new EventHandler(reportComponentSolucionadosClick);
            reporteVisualControl.pnl_fondoTime.Click += new EventHandler(reportComponentSolucionadosClick);  

            return reporteVisualControl;
        }
        public void reportComponentSolucionadosClick(Object sender, EventArgs e)
        {
            try
            {
                Control objeto = (Control)sender;
                Control parent = FindControlParent(objeto, "UserControlSol");
                ReporteSolucionadoVisual repoVisual = ((WindowsFormsControlLibrary1.UserControlSol)parent).Reporte;
                this.MostrarInformacionReporteSolucionados(repoVisual);
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message);
            }
        }

        public void MostrarInformacionReporteSolucionados(ReporteSolucionadoVisual reporte)
        {
            this.LimpiarDatosInformacionReporteSolucionado();        
            this.txt_numeroSol.Text = reporte.Numero;
            this.txt_equipoSol.Text = reporte.NombreEquipo;
            this.txt_pcOrigenSol.Text = reporte.NombrePC;
            this.txt_departamentoSol.Text = reporte.Departamento;
            this.txt_observacionSol.Text = reporte.Observacion;
            this.txt_usuarioSol.Text = reporte.NombreCliente;
            this.txt_administradorSol.Text = reporte.NombreAdministrador;
            this.txt_problemaSol.Text = reporte.ProblemaPosibleText;
            this.txt_solucionSol.Text = reporte.Solucion;
        }
        public void LimpiarDatosInformacionReporteSolucionado()
        {
            this.txt_numeroSol.Text = String.Empty;
            this.txt_equipoSol.Text = String.Empty;
            this.txt_pcOrigenSol.Text = String.Empty;
            this.txt_departamentoSol.Text = String.Empty;
            this.txt_observacionSol.Text = String.Empty;
            this.txt_usuarioSol.Text = String.Empty;
            this.txt_administradorSol.Text = String.Empty;
            this.txt_problemaSol.Text = String.Empty;
            this.txt_solucionSol.Text = String.Empty;
        }

        public void reportComponentClick(Object sender, EventArgs e)
        {
            try
            {
                Control objeto = (Control)sender;
                Control parent = FindControlParent(objeto, "UserControl1");
                ReporteVisual repoVisual = ((WindowsFormsControlLibrary1.UserControl1)parent).Reporte;

                Reporte repo = controlRepo.GetReportePorId(repoVisual.IdReporte);
                if (repo != null)
                {
                    repoVisual.NombreAdministrador = (repo.Administrador != null) ? repo.Administrador.nombre : String.Empty;
                    this.MostrarInformacionReporte(repoVisual);
                }
                else 
                { 
                    //repoVisual.NombreAdministrador = TEXT_SIN_ADMINISTRADOR; 
                    MessageBox.Show("Este reporte está siendo modificado por otra persona. Espere a que se actualice la lista de reportes pendientes y vuelva a intentarlo.");
                }                
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message);
            }

        }
        /// <summary>
        /// Se ejecuta al dar click sobre la imagen del administrador para cambiarla
        /// </summary>
        public void pict_administrador_Click(Object sender, EventArgs e)
        {
            try
            {
                Control objeto = (Control)sender;
                Control parent = FindControlParent(objeto, "UserControl1");
                WindowsFormsControlLibrary1.UserControl1 controlAdminVisual = (WindowsFormsControlLibrary1.UserControl1)parent;
                ImagenSelector imgSelector = new ImagenSelector(this.controlRepo,1);
                imgSelector.ShowDialog();
                Image imgSelected = imgSelector.ImagenAdminSelected;
                Reporte repo = controlRepo.GetReportePorId(controlAdminVisual.Reporte.IdReporte);
                if (imgSelected != null)//si seleccionó una imagen en el formulario ImagenSelector
                {

                    this.controlRepo.CambiarAdministradorYEstado(controlAdminVisual.Reporte.IdReporte, Estados.SiendoDefectado, imgSelector.AdminSelected.IdAdministrador);
                    controlAdminVisual.ImagenAdministrador = imgSelected;
                    if (repo != null)
                    {
                        controlAdminVisual.Reporte.NombreAdministrador = repo.Administrador.nombre;
                        controlAdminVisual.Reporte.Estado = repo.estado;
                    }
                    else controlAdminVisual.Reporte.NombreAdministrador = TEXT_SIN_ADMINISTRADOR;

                    controlAdminVisual.NombreAdministrador = controlAdminVisual.Reporte.NombreAdministrador;
                    controlAdminVisual.NombreAdminColor = COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO;
                    controlAdminVisual.SetColorNormalFondo(this.COLOR_REPORTEVISUAL_CON_ADMINISTRADOR);
                    if (repo != null && repo.ProblemaPosible != null)
                    {
                        if((bool) repo.ProblemaPosible.consumible)
                        {
                            controlAdminVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_COMSUMIBLE);
                        }
                        else
                        {
                            controlAdminVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR);
                        }
                    }
                    else
                    {
                        controlAdminVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR);
                    }                    
                    this.MostrarInformacionReporte(controlAdminVisual.Reporte);
                }
                if (imgSelector.DeseleccionarAdministrador)//seleccionó para quitar el administrador
                {
                    this.controlRepo.CambiarAdministradorYEstado(controlAdminVisual.Reporte.IdReporte, Estados.PendienteADefectar, -1);//se pasa -1 IdAdministrador para indicar ke debe eliminarse la relacion de la bd
                    controlAdminVisual.Reporte.NombreAdministrador = String.Empty;
                    controlAdminVisual.Reporte.Estado = Estados.PendienteADefectar;
                    this.MostrarInformacionReporte(controlAdminVisual.Reporte);
                    controlAdminVisual.NombreAdministrador = TEXT_SIN_ADMINISTRADOR;
                    controlAdminVisual.NombreAdminColor = COLOR_NOMBRE_ADMINISTRADOR_SINASIGNAR;
                    controlAdminVisual.SetColorNormalFondo(this.COLOR_REPORTEVISUAL_SIN_ADMINISTRADOR);
                    if (repo != null && repo.ProblemaPosible != null)
                    {
                        if((bool)repo.ProblemaPosible.consumible)
                        {
                            controlAdminVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_COMSUMIBLE);
                        }
                        else
                        {
                            controlAdminVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR);
                        }                        
                    }
                    else
                    {
                        controlAdminVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR);
                    }
                    controlAdminVisual.SetImagenDefecto();
                }
            }
            catch (CustomException msg)
            {
                MessageBox.Show(msg.Mensaje);
            }
        }

        public void ResetearReportes()
        {
            this.AdicionarReporteVisual(null);
            DateTime fechaUltimoRepoPendienteTraido = new DateTime(100, 1, 1);
            List<Reporte> reportes = controlRepo.GetReportesPendientesPosterioresA(fechaUltimoRepoPendienteTraido).OrderBy(x => x.orden).ToList();
            String cabecera;
            String descripcion;
            String entorno;
            String fechaHora;
            String nombreAdministrdor;
            String departamento;
            String observacion;
            ReporteVisual repDentroRepoControl;
            String color;
            String colorFondo;
            String colorIndicadorIzq;

            foreach (Reporte repo in reportes)
            {
                if (repo.Administrador != null)
                {
                    nombreAdministrdor = repo.Administrador.nombre;
                    color = COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO;
                    colorFondo = COLOR_REPORTEVISUAL_CON_ADMINISTRADOR;
                    if (repo.ProblemaPosible != null)
                    {
                        if ((bool)repo.ProblemaPosible.consumible)
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_COMSUMIBLE;
                        }
                        else
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR;
                        }

                    }
                    else
                    {
                        colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR;
                    }

                }
                else
                {
                    nombreAdministrdor = String.Empty;
                    color = COLOR_NOMBRE_ADMINISTRADOR_SINASIGNAR;
                    colorFondo = COLOR_REPORTEVISUAL_SIN_ADMINISTRADOR;
                    if (repo.ProblemaPosible != null)
                    {
                        if ((bool)repo.ProblemaPosible.consumible)
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_COMSUMIBLE;
                        }
                        else
                        {
                            colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR;
                        }

                    }
                    else
                    {
                        colorIndicadorIzq = COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR;
                    }
                }
                if (repo.orden != null)
                {
                    repDentroRepoControl = new ReporteVisual(repo.idReporte, repo.numero, repo.Equipo.nombre, repo.ProblemaPosible.problemaInfo,
                    repo.fecha_hora, repo.nombreCliente, repo.estado, repo.nombrePC, nombreAdministrdor, repo.departamento, repo.observacion);
                }
                else
                {
                    repDentroRepoControl = new ReporteVisual(repo.idReporte, repo.numero, repo.Equipo.nombre, repo.ProblemaPosible.problemaInfo,
                    repo.fecha_hora, repo.nombreCliente, repo.estado, repo.nombrePC, nombreAdministrdor, repo.departamento, repo.observacion);
                }
                cabecera = repo.numero + "  " + repo.nombreCliente + " : " + repo.departamento;
                descripcion = repo.Equipo.nombre + " : " + repo.ProblemaPosible.problemaInfo;
                fechaHora = repo.fecha_hora.ToString("hh:mm tt dd/M/yyyy", CultureInfo.InvariantCulture);
                if (repo.Entorno == null)
                    entorno = String.Empty;
                else entorno = repo.Entorno.infoEntorno;
                departamento = repo.departamento;
                observacion = repo.observacion;

                WindowsFormsControlLibrary1.UserControl1 controlRepVis = this.CrearControlReporteVisual(cabecera, descripcion, entorno, fechaHora, repDentroRepoControl, nombreAdministrdor);
                controlRepVis.NombreAdminColor = color;
                controlRepVis.SetColorNormalFondo(colorFondo);
                controlRepVis.SetColorIndicadorIzquierdo(colorIndicadorIzq);

                if (nombreAdministrdor != String.Empty && repo.Administrador.foto != null)
                {
                    if (repo.Administrador.foto != null)
                    {
                        Image imgAdmin = ImagenConvert.byteArrayToImage(repo.Administrador.foto);
                        controlRepVis.ImagenAdministrador = imgAdmin;
                    }
                }
                this.AdicionarReporteVisual(controlRepVis);
            }
        }

        public void solucionadoClick(Object sender, EventArgs e)
        {
            try
            {
                Control objeto = (Control)sender;
                Control parent = FindControlParent(objeto, "UserControl1");
                ReporteVisual repoVisual = ((WindowsFormsControlLibrary1.UserControl1)parent).Reporte;

                ImagenSelector imgSelector = null;
                if (repoVisual.NombreAdministrador == String.Empty || repoVisual.NombreAdministrador == TEXT_SIN_ADMINISTRADOR)//si no hay seleccionado un administrador
                {
                    imgSelector = new ImagenSelector(this.controlRepo, 2);
                    DialogResult result = imgSelector.ShowDialog();                  
                        if (imgSelector.AdminSelected != null && imgSelector.SolucionAdministrador != String.Empty)//si selecciono administrador y escribio solucion
                        {
                            this.controlRepo.CambiarAdministradorYEstado(repoVisual.IdReporte, Estados.Solucionado, imgSelector.AdminSelected.IdAdministrador);
                            String solucion = (imgSelector.SolucionAdministrador == String.Empty) ? null : imgSelector.SolucionAdministrador; //por si en algun momento se admite este campo en null (no es el caso para el cual se programa)
                            this.controlRepo.ConcluirReporte(repoVisual.IdReporte, solucion);
                            parent.Parent.Controls.Remove(parent);//se borra el componente de la lista
                        }                    
                }
                else //si ya hay un administrador seleccionado solo se muestra la ventana con la posibilidad de escribir la solucion
                {
                    imgSelector = new ImagenSelector(this.controlRepo, 3);
                    imgSelector.ShowDialog();
                        if (imgSelector.SolucionAdministrador != String.Empty)//si escribio una solucion
                        {
                            String solucion = (imgSelector.SolucionAdministrador == String.Empty) ? null : imgSelector.SolucionAdministrador; //por si en algun momento se admite este campo en null (no es el caso para el cual se programa)
                            this.controlRepo.ConcluirReporte(repoVisual.IdReporte, solucion);
                            parent.Parent.Controls.Remove(parent);//se borra el componente de la lista
                        }
                }
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message);
            }

            
        }
        public static Control FindControlParent(Control control, String type)
        {
            Control ctrlParent = control;
            while ((ctrlParent = ctrlParent.Parent) != null)
            {
                String tipo = ctrlParent.GetType().Name;
                if (tipo == type)
                {
                    return ctrlParent;
                }
            }
            return null;
        }
//---
        public void VerEntornos()
        {
            this.dtg_entornos.Rows.Clear();
            List<Entorno> entornos = this.controlRepo.GetEntornos();
           
            for (int i = 0; i < entornos.Count; i++)
            {
                this.dtg_entornos.Rows.Add();
                this.dtg_entornos.Rows[i].Cells[0].Value = entornos[i].idEntorno;
                this.dtg_entornos.Rows[i].Cells[1].Value = entornos[i].infoEntorno;
                this.dtg_entornos.Rows[i].Cells[2].Value = EDITAR_TEXT;
            }
        }

        public void VerEquipos()
        {
            this.dtg_equipo.Rows.Clear();
            List<Equipo> equipos = this.controlRepo.GetEquipos();

            for (int i = 0; i < equipos.Count; i++)
            {
                this.dtg_equipo.Rows.Add();
                this.dtg_equipo.Rows[i].Cells[0].Value = equipos[i].idEquipo;
                this.dtg_equipo.Rows[i].Cells[1].Value = equipos[i].nombre;
                this.dtg_equipo.Rows[i].Cells[2].Value = EDITAR_TEXT;
            }
        }

        public void MostrarPanelCRUDEntorno(PanelCRUD panelAMostrar, int idEntidad)
        {
            if (panelAMostrar == PanelCRUD.Editar)
            {
                try
                {
                    this.entornoSeleccionadoEditar = this.controlRepo.GetEntorno(idEntidad);
                    if (this.entornoSeleccionadoEditar != null)
                    {
                        Entorno ent = this.entornoSeleccionadoEditar;
                        this.txt_nombreEntornoEditar.Text = ent.infoEntorno;
                        this.check_entornoEditarEnUso.Checked = ((UsoEntidades)ent.desuso == UsoEntidades.DesUso);
                        this.MostrarPanel(this.pnl_editarEntorno, new List<Panel>() { this.pnl_adicionarEntorno, this.pnl_verEntornos });
                    }
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }

            }
            if (panelAMostrar == PanelCRUD.Adicionar)
            {
                this.MostrarPanel(this.pnl_adicionarEntorno);
            }
        }
        public void MostrarPanel(Panel panelABringToFront, List<Panel> panelesOcultar)
        {
            panelABringToFront.Visible = true;
            panelABringToFront.BringToFront();
            foreach (Panel panel in panelesOcultar)
            {
                panel.SendToBack();
                panel.Visible = false;
            }
        }
        public void MostrarPanel(Panel panelABringToFront)
        {
            panelABringToFront.Visible = true;
            panelABringToFront.BringToFront();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            String textEntornos = this.txt_nombreEntornoAdicionar.Text;
            if (textEntornos != String.Empty)
            {
                try
                {
                    Entorno entorno = this.controlRepo.AdicionarEntorno(textEntornos, Convert.ToInt32(this.check_entornoAdicionarEnUso.Checked));
                    
                    if (entorno != null) //si no ocurrio error adicionando el entorno
                    {
                        this.txt_nombreEntornoAdicionar.Text = String.Empty;
                        this.check_entornoAdicionarEnUso.Checked = false;
                        this.txt_nombreEntornoAdicionar.Focus();                        
                    }
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);                    
                }                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.txt_nombreEntornoEditar.Text != String.Empty)
            {
                try
                {
                    if (this.entornoSeleccionadoEditar != null)
                    {
                        this.entornoSeleccionadoEditar.infoEntorno = this.txt_nombreEntornoEditar.Text;
                        this.entornoSeleccionadoEditar.desuso = Convert.ToInt32(this.check_entornoEditarEnUso.Checked);
                        this.controlRepo.EditarEntorno(this.entornoSeleccionadoEditar);
                        MessageBox.Show("Edicion culminada exitosamente");
                    }
                    else MessageBox.Show("No se ha seleccionado un entorno para ser editado");
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }

            }
            else MessageBox.Show("Debe escribir un nombre de entorno.");
        }

        //--administrador
        public void VerAdministradores()
        {
            this.dtg_Administrador.Rows.Clear();
            List<Administrador> administradores = this.controlRepo.GetAdministradores();

            for (int i = 0; i < administradores.Count; i++)
            {
                this.dtg_Administrador.Rows.Add();
                this.dtg_Administrador.Rows[i].Cells[0].Value = administradores[i].idAdministrador;
                this.dtg_Administrador.Rows[i].Cells[1].Value = administradores[i].nombre;
                this.dtg_Administrador.Rows[i].Cells[2].Value = EDITAR_TEXT;
            }
        }

        public void VerTrabajadores()
        {
            //this.comboTrabajadores.;
            List<Administrador> administradores = this.controlRepo.GetAdministradores();

            for (int i = 0; i < administradores.Count; i++)
            {
                this.dtg_Administrador.Rows.Add();
                this.dtg_Administrador.Rows[i].Cells[0].Value = administradores[i].idAdministrador;
                this.dtg_Administrador.Rows[i].Cells[1].Value = administradores[i].nombre;
                this.dtg_Administrador.Rows[i].Cells[2].Value = EDITAR_TEXT;
            }
        }

        private void tab_CRUD_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page.Name == btn_adicionarEquipos.TabPages[0].Name) //si se selecciona la primera pagina (entornos)
            {
                this.VerEntornos();
                MostrarPanel(this.pnl_verEntornos, new List<Panel>() 
                { 
                    this.pnl_adicionarEntorno,
                    this.pnl_editarEntorno
                });
            }
            else
            if (e.Page.Name == btn_adicionarEquipos.TabPages[1].Name) //si se selecciona la 2da pagina (equipos)
            {
                this.VerEquipos();
                MostrarPanel(this.pnl_verEquipo, new List<Panel>() 
                { 
                    this.pnl_adicionarEquipo,
                    this.pnl_editarEquipo
                });
            }
            else
            if (e.Page.Name == btn_adicionarEquipos.TabPages[2].Name) //administradores
            {
                this.VerAdministradores();
                MostrarPanel(this.pnl_verAdministrador, new List<Panel>() 
                { 
                    this.pnl_adicionarAdministrador,
                    this.pnl_editarAdministrador
                });
            }
        }

        private void dtg_Administrador_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                if (dtg_Administrador.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == EDITAR_TEXT)//si se clickeo la celda para editar el entorno
                    MostrarPanelCRUDAdministrador(PanelCRUD.Editar, Convert.ToInt32(dtg_Administrador.Rows[e.RowIndex].Cells[0].Value));
            }
        }

        public void MostrarPanelCRUDAdministrador(PanelCRUD panelAMostrar, int idEntidad)
        {
            if (panelAMostrar == PanelCRUD.Editar)
            {
                try
                {
                    this.administradorSeleccionadoEditar = this.controlRepo.GetAdministrador(idEntidad);
                    if (this.administradorSeleccionadoEditar != null)
                    {
                        Administrador admin = this.administradorSeleccionadoEditar;
                        this.txt_nombreAdministradorEditar.Text = admin.nombre;
                        this.check_administradorEditarEnUso.Checked = ((UsoEntidades)admin.desuso == UsoEntidades.DesUso);
                        this.check_administradorEditarTecnico.Checked = (bool) admin.tecnico;
                        if (admin.foto != null)
                            this.SetImageBtn_foto(ImagenConvert.byteArrayToImage(admin.foto));
                        else this.CleanBtn_foto();
                        this.MostrarPanel(this.pnl_editarAdministrador, new List<Panel>() { this.pnl_adicionarAdministrador, this.pnl_verAdministrador});
                    }
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }
            }
            if (panelAMostrar == PanelCRUD.Adicionar)
            {
                this.MostrarPanel(this.pnl_adicionarAdministrador);
            }
        }

        private void btn_editarAdministrador_Click(object sender, EventArgs e)
        {
            if (this.txt_nombreAdministradorEditar.Text != String.Empty)//no se toma en cuenta la foto ya ke puede ser null este campo en la BD
            {
                try
                {
                    if (this.administradorSeleccionadoEditar != null)
                    {
                        this.administradorSeleccionadoEditar.nombre = this.txt_nombreAdministradorEditar.Text;
                        this.administradorSeleccionadoEditar.tecnico = this.check_administradorEditarTecnico.Checked;
                        this.administradorSeleccionadoEditar.desuso = Convert.ToInt32(this.check_administradorEditarEnUso.Checked);
                        if (this.btn_foto.BackgroundImage == null)//si no selecciono una foto
                            this.administradorSeleccionadoEditar.foto = null;
                        else this.administradorSeleccionadoEditar.foto = ImagenConvert.imageToByteArray(this.btn_foto.BackgroundImage);

                        this.controlRepo.EditarAdministrador(this.administradorSeleccionadoEditar);
                        MessageBox.Show("Edición culminada exitosamente");
                    }
                    else MessageBox.Show("No se ha seleccionado un administrador para ser editado");
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }

            }
            else MessageBox.Show("Debe escribir un nombre de administrador.");
        }

        private void btn_adicionarAdministrador_Click(object sender, EventArgs e)
        {
            String textNombre = this.txt_nombreAdministradorAdicionar.Text;
            if (textNombre != String.Empty)
            {
                try
                {
                    byte[] foto = (this.text_dirFotoAdminAdicionar.Text == String.Empty) ? null : ImagenConvert.imageToByteArray(Image.FromFile(this.text_dirFotoAdminAdicionar.Text));
                    Administrador administ = this.controlRepo.AdicionarAdministrador(textNombre, foto, Convert.ToInt32(this.check_administradorEditarEnUsoAdicionar.Checked), this.check_administradorEditarTecnicoAdicionar.Checked);

                    if (administ != null) //si no ocurrio error adicionando el administrador
                    {
                        this.txt_nombreAdministradorAdicionar.Text = String.Empty;
                        this.text_dirFotoAdminAdicionar.Text = String.Empty;
                        this.check_administradorEditarEnUsoAdicionar.Checked = false;
                        this.check_administradorEditarTecnicoAdicionar.Checked = false;
                        this.txt_nombreAdministradorAdicionar.Focus();
                    }
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }
            }
            else MessageBox.Show("Ingrese un nombre de administrador.");
        }

        private void dtg_entornos_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (dtg_entornos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == EDITAR_TEXT)//si se clickeo la celda para editar el entorno
                MostrarPanelCRUDEntorno(PanelCRUD.Editar, Convert.ToInt32(dtg_entornos.Rows[e.RowIndex].Cells[0].Value));
        }


        private void btn_adicionarEntornos_Click(object sender, EventArgs e)
        {            
            MostrarPanel(this.pnl_adicionarEntorno, new List<Panel>() 
            { 
                this.pnl_verEntornos,
                this.pnl_editarEntorno
            });
        }

        private void btn_verEntornos_Click(object sender, EventArgs e)
        {
            this.VerEntornos();
            MostrarPanel(this.pnl_verEntornos, new List<Panel>() 
            { 
                this.pnl_adicionarEntorno,
                this.pnl_editarEntorno
            });
        }

        private void btn_verAdministradores_Click(object sender, EventArgs e)
        {
            this.VerAdministradores();
            MostrarPanel(this.pnl_verAdministrador, new List<Panel>() 
            { 
                this.pnl_adicionarAdministrador,
                this.pnl_editarAdministrador
            });
        }

        private void btn_adicionarAdministradores_Click(object sender, EventArgs e)
        {
            MostrarPanel(this.pnl_adicionarAdministrador, new List<Panel>() 
            { 
                this.pnl_verAdministrador,
                this.pnl_editarAdministrador
            });
        }

        private void hyperLinkEdit1_OpenLink_1(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
        {
            MostrarPanel(this.pnl_adicionarEntorno, new List<Panel>() { this.pnl_verEntornos, this.pnl_editarEntorno});
        }

        private void hyperLinkEdit2_OpenLink(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
        {
            MostrarPanel(this.pnl_adicionarAdministrador, new List<Panel>() { this.pnl_verAdministrador, this.pnl_editarAdministrador });
        }

        private void worker_mostrarSolucionados_DoWork(object sender, DoWorkEventArgs e)
        {
            this.PrepararReporteVisualSolucionados();
        }

        private void worker_mostrarSolucionados_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.OcultarProgressCargando();
            this.TraerAlFrente(this.pnl_solucionados);
            this.txt_nombreFilter.Text = String.Empty;
        }

        private void btn_equipos_Click(object sender, EventArgs e)
        {
            this.VerEquipos();
            MostrarPanel(this.pnl_verEquipo, new List<Panel>() 
            { 
                this.pnl_adicionarEquipo,
                this.pnl_editarEquipo
            });
        }

        private void dtg_equipo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                if (dtg_equipo.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == EDITAR_TEXT)//si se clickeo la celda para editar el entorno
                    MostrarPanelCRUDEquipo(PanelCRUD.Editar, Convert.ToInt32(dtg_equipo.Rows[e.RowIndex].Cells[0].Value));
            }
        }

        public void MostrarPanelCRUDEquipo(PanelCRUD panelAMostrar, int idEntidad)
        {
            if (panelAMostrar == PanelCRUD.Editar)
            {
                try
                {
                    this.equipoSeleccionadoEditar = this.controlRepo.GetEquipo(idEntidad);
                    if (this.equipoSeleccionadoEditar != null)
                    {
                        Equipo equipo = this.equipoSeleccionadoEditar;                        
                        this.txt_nombreEquipoEditar.Text = equipo.nombre;
                        this.check_equipoEditar.Checked = ((UsoEntidades)equipo.desuso == UsoEntidades.DesUso);
                        this.MostrarProblemasEquipo(equipo, this.dtg_problemasPosibles);
                        this.MostrarPanel(this.pnl_editarEquipo, new List<Panel>() { this.pnl_adicionarEquipo, this.pnl_verEquipo });
                    }
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }

            }
            if (panelAMostrar == PanelCRUD.Adicionar)
            {
                this.MostrarPanel(this.pnl_adicionarAdministrador);
            }
        }

        public void MostrarProblemasEquipo(Equipo equipo, DataGridView listVisual)
        {
            listVisual.Rows.Clear();
            List<ProblemaPosible> problemas = equipo.ProblemaPosible.ToList<ProblemaPosible>();
            for (int i = 0; i < problemas.Count ; i++)
            {
                listVisual.Rows.Add();
                listVisual.Rows[i].Cells[0].Value = problemas[i].idProblemaPosible;
                listVisual.Rows[i].Cells[1].Value = problemas[i].problemaInfo;
                listVisual.Rows[i].Cells[2].Value = ((UsoEntidades)problemas[i].desuso == UsoEntidades.DesUso);
                listVisual.Rows[i].Cells[3].Value = problemas[i].consumible;
                listVisual.Rows[i].Cells[4].Value = String.Empty;
            }
        }

        private void btn_addProblema_Click(object sender, EventArgs e)
        {
            if (this.txt_problemaAdd.Text != String.Empty)
            {
                this.dtg_problemasPosibles.Rows.Add();
                this.dtg_problemasPosibles.Rows[dtg_problemasPosibles.Rows.Count -1].Cells[0].Value = -1;
                this.dtg_problemasPosibles.Rows[dtg_problemasPosibles.Rows.Count -1].Cells[1].Value = this.txt_problemaAdd.Text;
                this.dtg_problemasPosibles.Rows[dtg_problemasPosibles.Rows.Count -1].Cells[2].Value = (UsoEntidades.Uso);
                this.dtg_problemasPosibles.Rows[dtg_problemasPosibles.Rows.Count -1].Cells[3].Value = this.checkBox1.Checked;
                this.dtg_problemasPosibles.Rows[dtg_problemasPosibles.Rows.Count -1].Cells[4].Value = QUITAR_TEXT;
                this.checkBox1.Checked = false;
                this.txt_problemaAdd.Text = String.Empty;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            String nombre = this.txt_nombreEquipoEditar.Text;
            if (nombre != String.Empty)
            {
                if (equipoSeleccionadoEditar != null)
                {
                    equipoSeleccionadoEditar.nombre = nombre;
                    equipoSeleccionadoEditar.desuso = Convert.ToInt32(this.check_equipoEditar.Checked);
                    if (this.controlRepo.EditarEquipo(this.EditarProblemasPosibles(this.equipoSeleccionadoEditar)))
                        MessageBox.Show("Proceso de edición culminado satisfactoriamente");
                    else MessageBox.Show("Ocurrió un error mientras se editaba el equipo");

                }
                else MessageBox.Show("No se ha seleccionado un equipo para ser editado.");
            }
            else MessageBox.Show("Debe escribir un nombre.");
        }

        private Equipo EditarProblemasPosibles(Equipo equipo)
        {
            DataGridViewRowCollection rows =  this.dtg_problemasPosibles.Rows;
            for (int i = 0; i < rows.Count ; i++)
            {
                if ((int)rows[i].Cells[0].Value > -1)//si no es un problemaPosible acabado de adicionar al datagrid
                {
                    foreach (ProblemaPosible problema in equipo.ProblemaPosible)
                    {
                        if (problema.idProblemaPosible == (int)rows[i].Cells[0].Value)//si el problema posible existe en la lista de problemas posibles del equipo, entonces edito
                        {
                            problema.problemaInfo = rows[i].Cells[1].Value.ToString();
                            problema.desuso = Convert.ToInt32(((DataGridViewCheckBoxCell)rows[i].Cells[2]).Value);
                            problema.consumible = Convert.ToBoolean(rows[i].Cells[3].Value);
                        }
                    }
                }
                else 
                {
                    ProblemaPosible problem = this.controlRepo.CrearProblemaPosible(rows[i].Cells[1].Value.ToString(), false);
                    problem.desuso = Convert.ToInt32(((DataGridViewCheckBoxCell)rows[i].Cells[2]).Value);
                    problem.consumible = Convert.ToBoolean(rows[i].Cells[3].Value);
                    equipo.ProblemaPosible.Add(problem);
                }
            }
            return equipo;
        }

        private void dtg_problemasPosibles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                if (dtg_problemasPosibles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == QUITAR_TEXT)//si se clickeo la celda para quitar la fila
                    dtg_problemasPosibles.Rows.RemoveAt(e.RowIndex);                                      
            }
        }

        private void hyperLinkEdit3_OpenLink(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
        {
            MostrarPanel(this.pnl_adicionarEquipo, new List<Panel>() 
            { 
                this.pnl_editarEquipo,
                this.pnl_verEquipo
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.txt_problemaEquipoAdicionar.Text != String.Empty)
            {
                this.dtg_problemaAdicionar.Rows.Add();
                this.dtg_problemaAdicionar.Rows[dtg_problemaAdicionar.Rows.Count - 1].Cells[0].Value = -1;
                this.dtg_problemaAdicionar.Rows[dtg_problemaAdicionar.Rows.Count - 1].Cells[1].Value = this.txt_problemaEquipoAdicionar.Text;
                this.dtg_problemaAdicionar.Rows[dtg_problemaAdicionar.Rows.Count - 1].Cells[2].Value = (UsoEntidades.Uso);
                this.dtg_problemaAdicionar.Rows[dtg_problemaAdicionar.Rows.Count - 1].Cells[3].Value = this.checkBox2.Checked;
                this.dtg_problemaAdicionar.Rows[dtg_problemaAdicionar.Rows.Count - 1].Cells[4].Value = QUITAR_TEXT;
                this.txt_problemaEquipoAdicionar.Text = String.Empty;
            }
        }

        private void dtg_problemaAdicionar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                if (dtg_problemaAdicionar.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == QUITAR_TEXT)//si se clickeo la celda para quitar la fila
                    dtg_problemaAdicionar.Rows.RemoveAt(e.RowIndex);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Equipo equipo; 
            if (this.txt_nombreEquipoAdicionar.Text != String.Empty)
            {
                equipo = this.controlRepo.CrearEquipo(this.txt_nombreEquipoAdicionar.Text);
                equipo.desuso = Convert.ToInt32(this.check_equipoadicionar.Checked);
                this.SetProblemasAEquipo(equipo);
                if (!this.controlRepo.InsertarEquipo(equipo))
                    MessageBox.Show("Ocurrió un problema insertando en la base de datos.");
                else this.LimpiarPantallaAdicionarEquipo();
            }
            else
                MessageBox.Show("Debe escribir un nombre.");
        }
        public Equipo SetProblemasAEquipo(Equipo equipo)
        {
            DataGridViewRowCollection rows = this.dtg_problemaAdicionar.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                ProblemaPosible problem = this.controlRepo.CrearProblemaPosible(rows[i].Cells[1].Value.ToString(), false);
                problem.desuso = Convert.ToInt32(((DataGridViewCheckBoxCell)rows[i].Cells[2]).Value);
                problem.consumible = Convert.ToBoolean((rows[i].Cells[3]).Value);
                equipo.ProblemaPosible.Add(problem);
            }
            return equipo;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MostrarPanel(this.pnl_adicionarEquipo, new List<Panel>() 
            { 
                this.pnl_editarEquipo,
                this.pnl_verEquipo
            });
        }
        private void LimpiarPantallaAdicionarEquipo()
        {
            txt_nombreEquipoAdicionar.Text = String.Empty;
            txt_problemaEquipoAdicionar.Text = String.Empty;
            this.check_equipoadicionar.Checked = false;
            this.checkBox2.Checked = false;
            this.dtg_problemaAdicionar.Rows.Clear();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String dir = AbrirDialogoSelectorImagen();
            if (dir != null)
                this.text_dirFotoAdminAdicionar.Text = dir;
            else MessageBox.Show("Ocurrió un error cargando la imagen.");
        }
        /// <summary>
        /// retorna la direccion obtenida al abrir el filedialog o Null de ocurrir error        
        /// </summary>
        /// <returns></returns>
        public String AbrirDialogoSelectorImagen()
        {
            this.openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            this.openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif";
            String dir = String.Empty; 
            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    dir = this.openFileDialog1.FileName;
                }
                catch (Exception)
                {
                    dir = null;                    
                }
            }
            return dir;
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!this.controlRepo.CheckearAnclaje())
                this.Close();
        }

        private void btn_configurar_Click(object sender, EventArgs e)
        {
            loggin logginForm = new loggin(this.controlRepo);
            logginForm.ShowDialog();
            if (logginForm.passOK)
            {
                this.VerEntornos();
                this.TraerAlFrente(this.pnl_administrar);
                this.lbl_titulo.Text = "Administrar";
            }
        }

        private void btn_foto_Click(object sender, EventArgs e)
        {
            String dir = AbrirDialogoSelectorImagen();
            if (dir != null)
            {
                if (dir != String.Empty)
                    this.SetImageBtn_foto(Image.FromFile(dir));
            }
            else MessageBox.Show("Ocurrió un error cargando la imagen.");
        }
        public void SetImageBtn_foto(Image imagen)
        {
            this.btn_foto.Image = null;
            this.btn_foto.BackgroundImage = imagen;
            this.btn_foto.Refresh();
            this.btn_foto.ResumeLayout();
        }
        public void CleanBtn_foto()
        {
            this.btn_foto.BackgroundImage = null;
            this.btn_foto.Image = global::ReportesApp.Properties.Resources.Usericon2;

        }

        delegate void ActualizarReporteVisualPeriodicamente_delegado();
        public void ActualizarReporteVisualPeriodicamente()
        {
            if (this.splitContainerControl1.Panel1.InvokeRequired)
            {
                ActualizarReporteVisualPeriodicamente_delegado d = new ActualizarReporteVisualPeriodicamente_delegado(ActualizarReporteVisualPeriodicamente);
                this.Invoke(d, new object[] { });
            }
            else
            {
                List<Reporte> reposPendientes = controlRepo.GetReportesPendientes().OrderBy(x => x.orden).ToList();
                for (int i = 0; i < this.splitContainerControl1.Panel1.Controls.Count; i++)
                {                    
                    if (this.ActualizarEstadoRepoVisual((WindowsFormsControlLibrary1.UserControl1)this.splitContainerControl1.Panel1.Controls[i],reposPendientes))//si borro el componente visual
                        i--;
                }
            }
        }

        delegate bool ActualizarEstadoRepoVisual_delegado(WindowsFormsControlLibrary1.UserControl1 controlVisual, List<Reporte> reposPendientes);
        /// <summary>
        /// retorna true en caso de haber borrado el componente visual
        /// </summary>
        /// <param name="controlVisual"></param>
        /// <returns></returns>
        public bool ActualizarEstadoRepoVisual(WindowsFormsControlLibrary1.UserControl1 controlVisual, List<Reporte> reposPendientes)
        {
            if (this.splitContainerControl1.Panel1.InvokeRequired)
            {
                ActualizarEstadoRepoVisual_delegado d = new ActualizarEstadoRepoVisual_delegado(ActualizarEstadoRepoVisual);
                this.Invoke(d, new object[] { controlVisual, reposPendientes });
            }
            else 
            {
                int i;
                for ( i = 0; i < reposPendientes.Count; i++)
			    {
                    Reporte reporte = reposPendientes[i];
                    //Controlador.CNX.Refresh(System.Data.Objects.RefreshMode.StoreWins, reporte);
                    Controlador.CNX.Entry(reporte).Reload();
                    if (controlVisual.Reporte.Numero == reporte.numero)//si existe el elemento en la lista visual
                    {
                        if (reporte.Administrador != null)//si el reporte de la bd tiene asignado un administrador
                        {
                            if (controlVisual.Reporte.NombreAdministrador != reporte.Administrador.nombre)//si el administrador no es el mismo entonces le pongo el nombre del administrador del reporte actualizado en la base de datos
                            {
                                controlVisual.Reporte.NombreAdministrador = reporte.Administrador.nombre;
                                controlVisual.Reporte.Estado = reporte.estado;
                                controlVisual.NombreAdministrador = controlVisual.Reporte.NombreAdministrador;
                                controlVisual.NombreAdminColor = COLOR_NOMBRE_ADMINISTRADOR_ASIGNADO;
                                controlVisual.SetColorNormalFondo(this.COLOR_REPORTEVISUAL_CON_ADMINISTRADOR);
                                if (reporte.ProblemaPosible != null)
                                {
                                    if((bool)reporte.ProblemaPosible.consumible)
                                    {
                                        controlVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR_COMSUMIBLE);
                                    }
                                    else
                                    {
                                        controlVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR);
                                    }
                                }
                                else
                                {
                                    controlVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_CON_ADMINISTRADOR);
                                }                                
                                if (reporte.Administrador.foto != null)
                                {
                                    Image imgAdmin = ImagenConvert.byteArrayToImage(reporte.Administrador.foto);
                                    controlVisual.ImagenAdministrador = imgAdmin;
                                }
                            }
                        }
                        else //si el reporte de la bd no tiene asignado un administrador
                        {
                            controlVisual.Reporte.NombreAdministrador = TEXT_SIN_ADMINISTRADOR;
                            controlVisual.Reporte.NombreAdministrador = String.Empty;
                            controlVisual.Reporte.Estado = Estados.PendienteADefectar;
                            controlVisual.NombreAdministrador = TEXT_SIN_ADMINISTRADOR;
                            controlVisual.NombreAdminColor = COLOR_NOMBRE_ADMINISTRADOR_SINASIGNAR;
                            controlVisual.SetColorNormalFondo(this.COLOR_REPORTEVISUAL_SIN_ADMINISTRADOR);
                            if (reporte.ProblemaPosible != null)
                            {
                                if ((bool)reporte.ProblemaPosible.consumible)
                                {
                                    controlVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR_COMSUMIBLE);
                                }
                                else
                                {
                                    controlVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR);
                                }
                            }
                            else
                            {
                                controlVisual.SetColorIndicadorIzquierdo(this.COLOR_INDICADORIZQUIERDO_SIN_ADMINISTRADOR);
                            }
                            controlVisual.SetImagenDefecto();
                        }
                        return false;
                    }
			    }
                //si no hay reportes en la base de datos o sino se encontro coincidencia, o sea ke no existe el reporte en la bd, entonces se borra el visual
                this.splitContainerControl1.Panel1.Controls.Remove(controlVisual);
                
                //se retorna true indicando ke se borro el reporte visual
                return true;
            }
            return true;
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            Ranking rankingForm = new Ranking(this.controlRepo);
            rankingForm.ShowDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.tileItem2_ItemClick_1(this, null);
        }

        private void buttonAñadirReporte_Click(object sender, EventArgs e)
        {
            var servicio = new Service1Client();
            var problema = comboProblema.SelectedValue;
            var trabajador = this.comboTrabajadores.Text;
            var nombrePC = this.textEditNombrePC.Text;
            var observacion = this.textObservacion.Text;

            if (trabajador == null || (trabajador.Equals("")))
            {
                MessageBox.Show("No ha seleccionado un trabajador, por favor seleccione uno");
            }
            else
            if (problema == null)
            {
                MessageBox.Show("No ha seleccionado un problema, por favor seleccione uno");
            }
            else
            {
                var nuevoReporte = new Reporte()
                {
                    departamento = servicio.DameNombreDepartamentoPersonaxExp((int)comboTrabajadores.SelectedValue),
                    estado = "p",
                    fecha_hora = DateTime.Now,
                    observacion = observacion,
                    nombrePC = nombrePC,
                    idEquipo = (int) comboEquipo.SelectedValue,
                    numero = GetAndSetNextNumero(),
                    idProblemaPosible = (int) problema,
                    nombreCliente = trabajador
                };
                try
                {
                    cnx.Reporte.Add(nuevoReporte);
                    cnx.SaveChanges();
                }
                catch (Exception msg)
                {
                    throw new Exception("Ocurrió un error adicionando el reporte en la BD: " + msg.Message);
                }
            }

        }

        public ReporteDBEntities cnx
        {
            get
            {
                if (_cnx == null)
                    _cnx = new ReporteDBEntities();
                return _cnx;
            }
            set { _cnx = value; }
        }
        
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

        public Consecutivo GetAndSetNext()
        {
            int incremento = 1;
            Consecutivo conActual = this.GetValorActualConsecutivo();
            if (conActual.consecutivoSecuencia >= 999)
                incremento = conActual.consecutivoSecuencia * -1;
            conActual.consecutivoSecuencia += incremento;
            cnx.SaveChanges();

            return conActual;
        }
        public String GetAndSetNextNumero()
        {
            Consecutivo cons = GetAndSetNext();
            String dtn = DateTime.Now.ToString("MMyy");
            String numero = cons.consecutivoSecuencia.ToString() + dtn.Substring(0, 2) + dtn.Substring(2, 2);

            return numero;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: esta línea de código carga datos en la tabla 'reporteDBDataSet1.Equipo' Puede moverla o quitarla según sea necesario.
            this.equipoTableAdapter.Fill(this.reporteDBDataSet1.Equipo);
            // TODO: esta línea de código carga datos en la tabla 'reporteDBDataSet.ProblemaPosible' Puede moverla o quitarla según sea necesario.
            this.problemaPosibleTableAdapter.Fill(this.reporteDBDataSet.ProblemaPosible);

        }

        private void comboEquipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboEquipo.SelectedValue != null)
            {
                problemaPosibleBindingSource.DataSource = controlProblema.GetProblemasPosiblesEnUsoPorEquipo((int)comboEquipo.SelectedValue);
            }            
        }
    }
}
