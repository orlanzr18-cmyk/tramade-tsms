using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public interface ICategoriaRepository
    {
        List<clsCategoriaModel> ObtenerCategorias();
        int ObtenerIdPorNombre(SqlConnection con, string nombre);
    }

    public interface IProveedorRepository
    {
        List<clsProveedorModel> ObtenerProveedores();
        int ObtenerIdPorNombre(SqlConnection con, string nombre);
    }

    public interface ISucursalRepository
    {
        List<clsSucursalModel> ObtenerSucursales();
        int ObtenerIdPorNombre(SqlConnection con, string nombre);
    }

    /// <summary>Reemplazo de las consultas de categorías en "clsConsultasInv".</summary>
    public class CategoriaRepository : ICategoriaRepository
    {
        public List<clsCategoriaModel> ObtenerCategorias()
        {
            var obj = new clsConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_CATEGORIAS", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => new clsCategoriaModel
                    {
                        Id = r.Table.Columns.Contains("id_categoria") ? System.Convert.ToInt32(r["id_categoria"]) : 0,
                        Nombre = r["nombre_categoria"].ToString()
                    }).ToList();
            }
            finally { obj.Cerrar(); }
        }

        public int ObtenerIdPorNombre(SqlConnection con, string nombre)
        {
            var cmd = new SqlCommand("PA_OBTENER_ID_CATEGORIA", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@nombre", nombre);
            return (int)cmd.ExecuteScalar();
        }
    }

    /// <summary>Reemplazo de las consultas de proveedores en "clsConsultasInv".</summary>
    public class ProveedorRepository : IProveedorRepository
    {
        public List<clsProveedorModel> ObtenerProveedores()
        {
            var obj = new clsConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_PROVEEDORES", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => new clsProveedorModel
                    {
                        Id = r.Table.Columns.Contains("id_proveedor") ? System.Convert.ToInt32(r["id_proveedor"]) : 0,
                        Nombre = r["nombre_comercial_proveedor"].ToString()
                    }).ToList();
            }
            finally { obj.Cerrar(); }
        }

        public int ObtenerIdPorNombre(SqlConnection con, string nombre)
        {
            var cmd = new SqlCommand("PA_OBTENER_ID_PROVEEDOR", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@nombre", nombre);
            return (int)cmd.ExecuteScalar();
        }
    }

    /// <summary>Reemplazo de las consultas de sucursales en "clsConsultasInv".</summary>
    public class SucursalRepository : ISucursalRepository
    {
        public List<clsSucursalModel> ObtenerSucursales()
        {
            var obj = new clsConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_SUCURSALES", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => new clsSucursalModel
                    {
                        Id = r.Table.Columns.Contains("id_sucursal") ? System.Convert.ToInt32(r["id_sucursal"]) : 0,
                        Nombre = r["nombre_sucursal"].ToString()
                    }).ToList();
            }
            finally { obj.Cerrar(); }
        }

        public int ObtenerIdPorNombre(SqlConnection con, string nombre)
        {
            var cmd = new SqlCommand("PA_OBTENER_ID_SUCURSAL", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@nombre", nombre);
            return (int)cmd.ExecuteScalar();
        }
    }
}
