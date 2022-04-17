using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ReportesApp
{
    public partial class Ranking : DevExpress.XtraEditors.XtraForm
    {
        ControlReporteadorUsuario controlRepo;
        public Ranking(ControlReporteadorUsuario controlRepo)
        {
            InitializeComponent();
            this.controlRepo = controlRepo;
            this.PrepararReporteRanking();
        }
        
        public void PrepararReporteRanking()
        {
            try
            {
                controlRanking.ControlRanking controlRankingVisual;
                List<AdministradorCantSolucionados> adminSols = this.controlRepo.GetRankingAdministradores();
                List<AdministradorCantSolucionados> adminSolsUltimoMes = this.controlRepo.GetRankingAdministradoresApartirDe(new DateTime(DateTime.Today.Year,DateTime.Today.Month,1));
                int i = adminSols.Count;
                foreach (AdministradorCantSolucionados adminsol in adminSols)
                {
                    if (adminsol.Administrador != null && adminsol.Administrador.nombre != String.Empty)
                    {
                        String cantidadesLiteral = "Total: " + adminsol.CantSolucionados;
                        AdministradorCantSolucionados adminSolUltMes = adminSolsUltimoMes.Find(delegate(AdministradorCantSolucionados admin)
                                                                      {
                                                                          return admin.Administrador.nombre == adminsol.Administrador.nombre;
                                                                      });
                        if (adminSolUltMes != null)//si existe una cantidad para este mes del administrador en esta iteracion del ciclo
                            cantidadesLiteral += ", Este mes: " + adminSolUltMes.CantSolucionados;
                        controlRankingVisual = this.CrearRankingControl(i--, cantidadesLiteral, adminsol.Administrador);
                        if (controlRankingVisual != null)
                            this.pnl_contenedor.Controls.Add(controlRankingVisual);
                    }
                }
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message);
            }
        }

        public controlRanking.ControlRanking CrearRankingControl(int numero, String cabecera, Administrador administrador)
        {
            controlRanking.ControlRanking controlRankingVisual = new controlRanking.ControlRanking();
            try
            {
                controlRankingVisual.CabeceraText = cabecera;
               // controlRankingVisual.ColorHoverName = /*"Gainsboro";*/"WhiteSmoke";
                controlRankingVisual.ColorNameFondo = "WhiteSmoke";
                controlRankingVisual.ColorNormalAdministrador = "DarkGray";
                controlRankingVisual.DescripcionText = "Reportes solucionados";
                controlRankingVisual.Cabecera.BackColor = Color.FromName("MediumTurquoise");
                controlRankingVisual.Dock = System.Windows.Forms.DockStyle.Top;
                controlRankingVisual.NombreAdminColor = "DimGray";
                controlRankingVisual.NombreAdministrador = administrador.nombre;
                controlRankingVisual.Numero = numero.ToString();

                if (administrador != null && administrador.nombre != String.Empty && administrador.foto != null)
                {
                    Image imgAdmin = ImagenConvert.byteArrayToImage(administrador.foto);
                    controlRankingVisual.ImagenAdministrador = imgAdmin;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return controlRankingVisual;
        }
    }
}