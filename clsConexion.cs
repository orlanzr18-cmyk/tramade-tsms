using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE
{
    internal class clsConexion
    {

        string conexion = "Data Source=ORLAN\\ORLANSERVER;" +
                  "Initial Catalog=DB_TRAMADE;" +
                  "Integrated Security=True;" +
                  "Persist Security Info=False;" +
                  "Pooling=False;" +
                  "MultipleActiveResultSets=False;" +
                  "Encrypt=True;" +
                  "TrustServerCertificate=True;" +
                  "Application Name=\"SQL Server Management Studio\";";

        public SqlConnection SqlC = new SqlConnection();
        public clsConexion()
        {
            SqlC.ConnectionString = conexion;
        }
        public void Abrir()
        {
            try
            {
                if (SqlC.State == ConnectionState.Closed)
                {
                    SqlC.Open();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public async Task AbrirAsync()
        {
            if (SqlC.State == ConnectionState.Closed || SqlC.State == ConnectionState.Broken)
            {
                await SqlC.OpenAsync();
            }
        }

        public void Cerrar()
        {
            try
            {
                if (SqlC.State == System.Data.ConnectionState.Open)
                {
                    SqlC.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
