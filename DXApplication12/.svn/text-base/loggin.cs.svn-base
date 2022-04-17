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
    public partial class loggin : DevExpress.XtraEditors.XtraForm
    {
        ControlReporteadorUsuario controlRepo;
        public bool passOK = false;

        public loggin(ControlReporteadorUsuario controlRepo)
        {
            InitializeComponent();

            this.controlRepo = controlRepo;
            if (!this.controlRepo.HayAccesosEnBD())
                this.TraerAlFrente(this.pnl_insertar,this.pnl_logueo);
            else this.TraerAlFrente(this.pnl_logueo,this.pnl_insertar);
        }

        private void btn_adicionarEntornos_Click(object sender, EventArgs e)
        {
            String pass = this.txt_passLoggin.Text;

            if (pass != String.Empty)
            {
                if (this.controlRepo.ContraseñaCorrecta(pass))
                {
                    this.passOK = true;
                    this.Close();
                }
                else 
                {
                    MessageBox.Show("La contraseña es incorrecta. Escriba nuevamente su contraseña");
                    this.txt_passLoggin.Text = String.Empty;
                    this.txt_passLoggin.Focus();
                }
            }
            else MessageBox.Show("Debe escribir una contraseña.");
        }

        public void TraerAlFrente(Panel panel, Panel ocultar)
        {
            panel.Visible = true;
            panel.BringToFront();
            ocultar.Visible = false;
            ocultar.SendToBack();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.txt_pass1.Text != String.Empty && this.txt_pass2.Text != String.Empty && this.txt_pass1.Text == this.txt_pass2.Text)
            {
                Acceso acc = this.controlRepo.AdicionarAcceso(this.txt_pass2.Text);
                if (acc != null)//si adiciono correctamente el acceso
                {
                    this.txt_pass2.Text = String.Empty;
                    this.txt_pass1.Text = String.Empty;
                    this.TraerAlFrente(this.pnl_logueo,this.pnl_insertar);
                }
            }
            else MessageBox.Show("Deben coincidir las contraseñas");
        }
    }
}