using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MySql.Data.MySqlClient;

namespace CatalogoPortalVideojuegos
{
    public partial class LoginWindow : Window
    {
        // Práctica 3: los usuarios ya no están en memoria, se validan contra la tabla Usuarios de MySQL.

        // Código de error de MySQL para clave duplicada (UNIQUE de nombre_usuario)
        private const int ERROR_DUPLICADO = 1062;

        private int intentosFallidos = 0;
        private const int MAX_INTENTOS = 3;
        private readonly DispatcherTimer timerBloqueo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };

        public LoginWindow()
        {
            InitializeComponent();
            timerBloqueo.Tick += (s, e) => DesbloquearAcceso();
        }

        // TC01, TC03, TC04, TC07, TC08, TC09: inicio de sesión contra la BBDD
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Password;

            // TC01: campos vacíos -> no se consulta la BBDD
            if (!CamposRellenos(user, pass)) return;

            MySqlConnection? conexion = null;
            try
            {
                conexion = DatabaseHelper.AbrirConexion();

                // Consulta parametrizada: los valores nunca se concatenan al SQL (TC09: inyección SQL)
                const string sql = "SELECT COUNT(*) FROM Usuarios " +
                                   "WHERE nombre_usuario = @usuario AND password = @password";
                using var comando = new MySqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("@usuario", user);
                comando.Parameters.AddWithValue("@password", pass);

                long coincidencias = Convert.ToInt64(comando.ExecuteScalar());

                // TC03 / TC07: login correcto
                if (coincidencias == 1)
                {
                    intentosFallidos = 0;
                    new MainWindow().Show();   // abre el catálogo
                    this.Close();              // cierra el login
                    return;
                }

                // TC04: usuario o contraseña incorrectos (no se dice cuál de los dos falla)
                intentosFallidos++;
                if (intentosFallidos >= MAX_INTENTOS)
                {
                    BloquearAcceso();
                }
                else
                {
                    Mostrar($"Usuario o contraseña incorrectos (intento {intentosFallidos}/{MAX_INTENTOS})", false);
                    txtPassword.Clear(); txtPassword.Focus();
                }
            }
            catch (MySqlException)
            {
                // TC08: MySQL parado -> aviso y la aplicación sigue abierta
                Mostrar("No se puede conectar con la base de datos. Compruebe que el servidor MySQL está en marcha.", false);
            }
            catch (Exception ex)
            {
                Mostrar("Error inesperado: " + ex.Message, false);
            }
            finally
            {
                DatabaseHelper.CerrarConexion(conexion);
            }
        }

        // TC05, TC06: alta de un usuario nuevo en la tabla
        private void btnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Password;

            if (!CamposRellenos(user, pass)) return;

            MySqlConnection? conexion = null;
            try
            {
                conexion = DatabaseHelper.AbrirConexion();

                const string sql = "INSERT INTO Usuarios (nombre_usuario, password) " +
                                   "VALUES (@usuario, @password)";
                using var comando = new MySqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("@usuario", user);
                comando.Parameters.AddWithValue("@password", pass);
                comando.ExecuteNonQuery();

                // TC05: alta correcta
                Mostrar($"Usuario '{user}' dado de alta correctamente. Ya puede iniciar sesión.", true, true);
                txtPassword.Clear(); txtPassword.Focus();
            }
            catch (MySqlException ex) when (ex.Number == ERROR_DUPLICADO)
            {
                // TC06: el UNIQUE de nombre_usuario impide el duplicado
                Mostrar("El usuario ya existe", false);
                txtUsuario.Focus(); txtUsuario.SelectAll();
            }
            catch (MySqlException)
            {
                // TC08: MySQL parado
                Mostrar("No se puede conectar con la base de datos. Compruebe que el servidor MySQL está en marcha.", false);
            }
            catch (Exception ex)
            {
                Mostrar("Error inesperado: " + ex.Message, false);
            }
            finally
            {
                DatabaseHelper.CerrarConexion(conexion);
            }
        }

        private bool CamposRellenos(string user, string pass)
        {
            if (user == "" || pass == "")
            {
                Mostrar("Introduzca los datos requeridos", false);
                return false;
            }
            return true;
        }

        private void BloquearAcceso()
        {
            txtUsuario.IsEnabled = txtPassword.IsEnabled = btnLogin.IsEnabled = btnNuevoUsuario.IsEnabled = false;
            Mostrar("Demasiados intentos. Acceso bloqueado 30 segundos.", false);
            timerBloqueo.Start();
        }

        private void DesbloquearAcceso()
        {
            timerBloqueo.Stop();
            intentosFallidos = 0;
            txtUsuario.IsEnabled = txtPassword.IsEnabled = btnLogin.IsEnabled = btnNuevoUsuario.IsEnabled = true;
            txtPassword.Clear();
            Mostrar("Acceso desbloqueado. Inténtelo de nuevo.", true);
        }

        // Muestra el mensaje en la etiqueta y, si es un error (o se pide), también por ventana
        private void Mostrar(string texto, bool ok, bool ventana = false)
        {
            lblMensaje.Text = texto;
            lblMensaje.Foreground = ok ? Brushes.LightGreen : Brushes.IndianRed;
            if (!ok)
                MessageBox.Show(texto, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (ventana)
                MessageBox.Show(texto, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // TC02: salir de la aplicación
        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
