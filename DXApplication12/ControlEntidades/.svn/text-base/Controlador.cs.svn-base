using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using ReportesApp;

namespace ReportesApp.ControlEntidades
{    
    class Controlador
    {
        static public ReporteDBEntities context = null;
        
        public Controlador()
        {
            if(context == null)
                context = new ReporteDBEntities();
        }
        private ReporteDBEntities GetCNX()
        {
            if (context == null)
                context = new ReporteDBEntities();
            return context;
        }
        public ReporteDBEntities cnx
        {
            get { return this.GetCNX(); }
            set { context = value; }
        }
        static public ReporteDBEntities CNX
        {
            get { return context; }
        }
    }
}
