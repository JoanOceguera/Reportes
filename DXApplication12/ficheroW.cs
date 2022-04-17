using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Permissions;

namespace StWork
{
    class ficheroW
    {
        private string direccion;
        private List<String> ultimoEscrito;
        private FileStream fichero;

        public ficheroW(string dir)
        {
            this.direccion = dir;
            this.ultimoEscrito = new List<String>();
        }

        public void SaveToFile(List<String> informacionList)
        {
            //    File.AppendAllText(this.direccion,informacion+Environment.NewLine,Encoding.UTF8);            
            this.fichero = new FileStream(this.direccion, FileMode.Append, FileAccess.Write, FileShare.Read);
           // BinaryWriter writer = new BinaryWriter(this.fichero);

            StreamWriter writer = new StreamWriter(this.fichero);
            foreach (String info in informacionList)
            {
                writer.Write(info);
                writer.Write(Environment.NewLine);
            }
            this.ultimoEscrito = informacionList;
            writer.Close();
            this.fichero.Close();
            
        }
        /// <summary>
        /// Lee del fichero y retorna una lista con los datos leidos, de ocurrir una IOExeption retorna null
        /// </summary>
        /// <returns></returns>
        public List<String> ReadFromFile()
        {
            this.fichero = new FileStream(this.direccion, FileMode.Open, FileAccess.Read, FileShare.Write);
            BinaryReader reader = new BinaryReader(this.fichero);
            List<String> datos = new List<string>();
            String leido;

            try
            {
                while (reader.PeekChar() != -1)
                {
                    int pekchar = reader.PeekChar();
                    leido = reader.ReadString();
                    datos.Add(leido);
                }
            }
            catch (IOException)
            {
                this.fichero.Close();
                reader.Close();
                return null;
            }

            this.fichero.Close();
            reader.Close();

            return (datos.Count == 0) ? null : datos;
        }
        public void ClearFile()
        {
            //  FileIOPermission f2 = new FileIOPermission(FileIOPermissionAccess.Read, "C:\\test_r");
            FileIOPermission permisos = new FileIOPermission(FileIOPermissionAccess.Write, this.direccion);
            this.fichero = new FileStream(this.direccion, FileMode.Truncate);
            this.fichero.Close();
        }

        public List<String> UltimoEscrito
        {
            get { return this.ultimoEscrito; }
        }

    }
}
