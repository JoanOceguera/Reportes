using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using PanelImagen;
using System.IO;

namespace ReportesApp
{
     partial class ImagenSelector : DevExpress.XtraEditors.XtraForm
    {
        private Image imagenAdminSelected;
        ControlReporteadorUsuario controlRepo;
        AdministradorVisual adminSelected;
        private bool deseleccionarAdministrador;
        private int modoApertura; //1-solo admins, 2- admins+solucion, 3-solo solucion
        int alturaMaxima = 330;
        int alturaMinima = 260;
        int alturaMinimaPequenna = 150;
        public String SolucionAdministrador = String.Empty;
        public bool nohacerNada = false;

        public bool DeseleccionarAdministrador
        {
            get { return deseleccionarAdministrador; }
            private set { deseleccionarAdministrador = value; }
        }        

        public AdministradorVisual AdminSelected
        {
            get { return adminSelected; }
            set { adminSelected = value; }
        }
        
        public ImagenSelector(ControlReporteadorUsuario controlRepom, int modoApertura)
        {
           
            this.controlRepo = controlRepom;
            this.modoApertura = modoApertura;
            InitializeComponent();
            

            if (this.modoApertura == 1)
                this.Height = this.alturaMinima;
            else if (this.modoApertura == 2)
                this.Height = this.alturaMaxima;
            else
            {
                this.pnl_superior.Visible = false;
                this.pnl_inferior.Visible = false;
                this.Height = this.alturaMinimaPequenna;
            }
        }

        public Image ImagenAdminSelected
        {
            get { return imagenAdminSelected; }
            set { imagenAdminSelected = value; }
        }

        public void MostrarAdministradores()
        {
            List<Administrador> administradoresEnUso = this.controlRepo.GetAdministradoresEnUso();
            AdministradorVisual adminVisual;
            LimpiarSuperiorInferior();
            PanelImagen.UserControl1 panelVisual;


            for (int i = 0; i < administradoresEnUso.Count; i++)
            {
                Administrador adm = administradoresEnUso[i];
                Image foto = null;
                if (adm.foto != null)
                    foto = ImagenConvert.byteArrayToImage(adm.foto);
                adminVisual = new AdministradorVisual(adm.idAdministrador, adm.nombre, foto);
                panelVisual = this.CrearPanelVisualImagen(adminVisual);
                if (i % 2 == 0)
                    this.pnl_superior.Controls.Add(panelVisual);
                else this.pnl_inferior.Controls.Add(panelVisual);
            }

            //se prepara un control para posibilitar deseleccionar un administrador
            adminVisual = new AdministradorVisual(-1, String.Empty, null);
            panelVisual = this.CrearPanelVisualImagen(adminVisual);
            this.pnl_superior.Controls.Add(panelVisual);
        }

        public void LimpiarSuperiorInferior()
        {
            this.pnl_inferior.Controls.Clear();
            this.pnl_superior.Controls.Clear();
        }
        public PanelImagen.UserControl1 CrearPanelVisualImagen(AdministradorVisual administradorVisual)
        {
            PanelImagen.UserControl1 pnlImg = new UserControl1();
            pnlImg.Administrador = administradorVisual;
            pnlImg.AdministradorText = administradorVisual.Nombre;
            //this.userControl18.ColorHoverName = "Silver";
            //this.userControl18.ColorNameFondo = "Gainsboro";
            pnlImg.Dock = System.Windows.Forms.DockStyle.Left;
            Image imgAdmin;
            if (administradorVisual.Foto != null)
            {
                imgAdmin = administradorVisual.Foto;
                pnlImg.ImagenAdministrador = imgAdmin;
            }
           // this.userControl18.Location = new System.Drawing.Point(240, 0);
           // this.userControl18.Name = "userControl18";
           // this.userControl18.Size = new System.Drawing.Size(80, 100);
           // this.userControl18.TabIndex = 8;

            pnlImg.pnl_fondo.Click += new EventHandler(ImagenComponentClick);
            pnlImg.txt_administrador.Click += new EventHandler(ImagenComponentClick);
            pnlImg.pict_administrador.Click += new EventHandler(ImagenComponentClick);

            return pnlImg;
        }

        public void ImagenComponentClick(Object sender, EventArgs e)
        {           
            Control objeto = (Control)sender;
            Control parent = Form1.FindControlParent(objeto, "UserControl1");
            PanelImagen.UserControl1 component = (PanelImagen.UserControl1)parent;
            Image pictAdmin = component.ImagenAdministrador;

            if (component.Administrador.IdAdministrador != -1)//si selecciono una foto de un administrador y no el componente por defecto para deseleccionar un administrador
            {
                this.ImagenAdminSelected = pictAdmin;
                this.AdminSelected = ((PanelImagen.UserControl1)parent).Administrador;
                this.lbl_adminSelected.Text = this.AdminSelected.Nombre;
            }
            else
            {
                this.DeseleccionarAdministrador = true;
                this.lbl_adminSelected.Text = String.Empty;
            }
            if(this.modoApertura == 1)
                this.Close();            
        }

        private void ImagenSelector_Resize(object sender, EventArgs e)
        {
            this.MostrarAdministradores();
        }

         private void btn_solucionarReporte_Click(object sender, EventArgs e)
         {             
             this.SolucionAdministrador = this.txt_solucion.Text;
             if (this.modoApertura == 2)
             {
                 if (this.AdminSelected == null && this.txt_solucion.Text == String.Empty)
                     MessageBox.Show("Debe seleccionar un administrador y escribir una solución.");
                 else if (this.AdminSelected == null)
                     MessageBox.Show("Debe seleccionar un administrador.");
                 else if (this.txt_solucion.Text == String.Empty)
                     MessageBox.Show("Debe escribir una solución.");
                 else this.Close();
             }
             else if (this.modoApertura == 3)
             {
                 if (this.txt_solucion.Text == String.Empty)
                     MessageBox.Show("Debe escribir una solución.");
                 else this.Close();
             }
             
         }

         private void txt_solucion_TextChanged(object sender, EventArgs e)
         {
             if (this.txt_solucion.TextLength <= 0)
                 this.SolucionAdministrador = String.Empty;
         }
    }
}