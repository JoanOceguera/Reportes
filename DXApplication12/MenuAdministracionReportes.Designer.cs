
using ReportesApp.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ReportesApp
{
    partial class MenuAdministracionReportes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new Panel();
            this.panel2 = new Panel();
            this.button2 = new Button();
            this.panel3 = new Panel();
            this.pnl_wait = new Panel();
            this.label3 = new Label();
            this.img_loadingPrenom = new PictureBox();
            this.worker_reportes = new BackgroundWorker();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnl_wait.SuspendLayout();
            ((ISupportInitialize)this.img_loadingPrenom).BeginInit();
            base.SuspendLayout();
            this.panel1.BackColor = Color.LightGray;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new Point(13, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(398, 334);
            this.panel1.TabIndex = 15;
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Location = new Point(1, 31);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(381, 30);
            this.panel2.TabIndex = 19;
            this.button2.BackColor = Color.FromArgb(0, 165, 223);
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = FlatStyle.Flat;
            this.button2.Font = new Font("Tahoma", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.button2.ForeColor = Color.Gainsboro;
            this.button2.Location = new Point(31, 0);
            this.button2.Name = "button2";
            this.button2.Size = new Size(350, 30);
            this.button2.TabIndex = 14;
            this.button2.Text = "Generar reporte de pendientes";
            this.button2.TextAlign = ContentAlignment.MiddleLeft;
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new EventHandler(this.button2_Click);
            this.panel3.BackColor = Color.Gold;
            this.panel3.Location = new Point(1, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new Size(7, 30);
            this.panel3.TabIndex = 15;
            this.pnl_wait.BackColor = Color.White;
            this.pnl_wait.Controls.Add(this.img_loadingPrenom);
            this.pnl_wait.Controls.Add(this.label3);
            this.pnl_wait.Location = new Point(12, 27);
            this.pnl_wait.Name = "pnl_wait";
            this.pnl_wait.Size = new Size(399, 334);
            this.pnl_wait.TabIndex = 5;
            this.pnl_wait.Visible = false;
            this.label3.AutoSize = true;
            this.label3.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.label3.ForeColor = Color.Gray;
            this.label3.Location = new Point(72, 64);
            this.label3.Name = "label3";
            this.label3.Size = new Size(240, 30);
            this.label3.TabIndex = 6;
            this.label3.Text = "Este Proceso puede tardar varios minutos. \r\nEspere por favor.";
            this.img_loadingPrenom.Image = Resources.loading11;
            this.img_loadingPrenom.Location = new Point(75, 126);
            this.img_loadingPrenom.Name = "img_loadingPrenom";
            this.img_loadingPrenom.Size = new Size(218, 26);
            this.img_loadingPrenom.TabIndex = 7;
            this.img_loadingPrenom.TabStop = false;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(425, 389);
            base.Controls.Add(this.pnl_wait);
            base.Controls.Add(this.panel1);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.Name = "MenuAdministracionReportes";
            base.ShowIcon = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Reportes";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.pnl_wait.ResumeLayout(false);
            this.pnl_wait.PerformLayout();
            ((ISupportInitialize)this.img_loadingPrenom).EndInit();
            base.ResumeLayout(false);
            this.worker_reportes.DoWork += new DoWorkEventHandler(this.worker_reportes_DoWork);
            this.worker_reportes.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_reportes_RunWorkerCompleted);
        }

        #endregion

        private void worker_reportes_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.icontrol.GenerarExcelReportes();
                this.pnl_wait.Visible = true;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private void worker_reportes_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.pnl_wait.Visible = false;
        }
    }
}