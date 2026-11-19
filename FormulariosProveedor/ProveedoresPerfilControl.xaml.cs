using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.Formulario_Proveedores.Clases;

namespace TRAMADE.FormulariosProveedor
{
    public partial class ProveedoresPerfilControl : UserControl
    {
        private int _idProveedor;
        private clsConexion _conexion = new clsConexion();

        public ProveedoresPerfilControl(int idProveedor)
        {
            InitializeComponent();
            _idProveedor = idProveedor;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (clsPermisos.esEmpleado)
                btnEditar.Visibility = Visibility.Collapsed;

            CargarPerfil();
        }
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var parent = System.Windows.Forms.Application.OpenForms["frmProveedores_Perfil"];
            parent?.Close();
        }

        public void CargarPerfil()
        {
            try
            {
                _conexion.Abrir();
                var cmd = new SqlCommand("PA_OBTENER_PERFIL_PROVEEDOR", _conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_proveedor", _idProveedor);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtIdProveedor.Text    = reader["id_proveedor"].ToString();
                    txtNombreComercial.Text = reader["nombre_comercial_proveedor"].ToString();
                    txtRazonSocial.Text    = reader["razon_social_proveedor"].ToString();
                    txtDireccion.Text      = reader["direccion_proveedor"].ToString();
                    txtRtn.Text            = reader["rtn_proveedor"].ToString();
                    txtContacto.Text       = reader["contacto_proveedor"].ToString();
                    txtCorreo.Text         = reader["correo_electronico_proveedor"].ToString();
                    txtClasificacion.Text  = reader["descripcion_clasificacion_proveedor"].ToString();
                    txtTerminosPago.Text   = reader["descripcion_terminos_de_pago"].ToString();
                    txtDiasCredito.Text    = reader["dias_credito"].ToString() + " días";
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar perfil: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            btnEditar.IsEnabled = false;

            try
            {
                var frmEditar = new frmProveedores_Editar(_idProveedor);
                frmEditar.FormClosed += (s, ev) => CargarPerfil();
                frmEditar.ShowDialog();
            }
            finally
            {
                btnEditar.IsEnabled = true;
            }
        }
    }
}
