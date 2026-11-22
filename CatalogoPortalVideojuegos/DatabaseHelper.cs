using MySql.Data.MySqlClient;

namespace CatalogoPortalVideojuegos
{
    /// <summary>
    /// Centraliza el acceso a MySQL: guarda la cadena de conexión
    /// y se encarga de abrir y cerrar las conexiones.
    /// </summary>
    public static class DatabaseHelper
    {
        // Usuario propio de la aplicación (creado en database.sql), no se usa root.
        // Connection Timeout corto para que, si MySQL está parado, el aviso salga enseguida.
        private const string CadenaConexion =
            "Server=localhost;Port=3306;Database=catalogo_videojuegos;" +
            "Uid=catalogo_app;Pwd=CatalogoApp2026!;" +
            "Connection Timeout=3;";

        /// <summary>
        /// Crea una conexión y la abre. Si MySQL no responde lanza MySqlException,
        /// que se captura en la ventana con try/catch.
        /// </summary>
        public static MySqlConnection AbrirConexion()
        {
            var conexion = new MySqlConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        /// <summary>
        /// Cierra y libera la conexión si estaba abierta.
        /// </summary>
        public static void CerrarConexion(MySqlConnection? conexion)
        {
            if (conexion == null) return;
            if (conexion.State != System.Data.ConnectionState.Closed)
                conexion.Close();
            conexion.Dispose();
        }
    }
}
