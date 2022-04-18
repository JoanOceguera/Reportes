using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReportesApp
{
    public partial class MenuAdministracionReportes : Form
    {
        private ControlReporteadorUsuario icontrol;

        private Panel panel1;

        private Panel panel2;

        private Button button2;

        private Panel panel3;

        private Panel pnl_wait;

        private Label label3;

        private PictureBox img_loadingPrenom;

        private BackgroundWorker worker_reportes;

        public MenuAdministracionReportes(ControlReporteadorUsuario icontrol)
        {
            this.icontrol = icontrol;
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.worker_reportes.RunWorkerAsync();
            this.pnl_wait.Visible = true;
        }
    }
}
