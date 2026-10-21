using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RegistroEstudiantesCarlos.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string MensajeSistema { get; set; } = "";

        public class Estudiante
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
            public string Apellido { get; set; } = "";
        }

        public List<Estudiante> ListaEstudiantes { get; set; } = new List<Estudiante>();

        private readonly string cadenaConexion = "Server=localhost\\SQLEXPRESS02;Database=EstudiantesDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public void OnGet()
        {
            CargarEstudiantes();
        }

        private void CargarEstudiantes()
        {
            ListaEstudiantes.Clear();
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    conexion.Open();
                    string consultaSql = "SELECT Id, Nombre, Apellido FROM Estudiantes";
                    using (SqlCommand comando = new SqlCommand(consultaSql, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Estudiante estudiante = new Estudiante
                                {
                                    Id = lector.GetInt32(0),
                                    Nombre = lector.GetString(1),
                                    Apellido = lector.GetString(2)
                                };
                                ListaEstudiantes.Add(estudiante);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeSistema = "Error al conectar con la base de datos: " + ex.Message;
            }
        }

        public IActionResult OnPost(string nombreForm, string apellidoForm, int estudiantesId, string accion)
        {
            try
            {
                if (accion == "guardar")
                {
                    if (string.IsNullOrEmpty(nombreForm) || string.IsNullOrEmpty(apellidoForm))
                    {
                        MensajeSistema = "Error: Debes llenar ambos campos.";
                        CargarEstudiantes();
                        return Page();
                    }

                    string consultaSql = "INSERT INTO Estudiantes (Nombre, Apellido) VALUES (@nom, @ape)";
                    using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                    {
                        conexion.Open();
                        using (SqlCommand comando = new SqlCommand(consultaSql, conexion))
                        {
                            comando.Parameters.AddWithValue("@nom", nombreForm);
                            comando.Parameters.AddWithValue("@ape", apellidoForm);
                            comando.ExecuteNonQuery();
                        }
                    }

                    MensajeSistema = $"¡Estudiante {nombreForm} {apellidoForm} registrado con éxito!";
                }
                else if (accion == "borrar_individual")
                {
                    string consultaSql = "DELETE FROM Estudiantes WHERE Id = @id";
                    using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                    {
                        conexion.Open();
                        using (SqlCommand comando = new SqlCommand(consultaSql, conexion))
                        {
                            comando.Parameters.AddWithValue("@id", estudiantesId);
                            comando.ExecuteNonQuery();
                        }
                    }

                    MensajeSistema = $"Registro con ID {estudiantesId} eliminado correctamente.";
                }
                else if (accion == "vaciar_tabla")
                {
                    string consultaSql = "DELETE FROM Estudiantes";
                    using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                    {
                        conexion.Open();
                        using (SqlCommand comando = new SqlCommand(consultaSql, conexion))
                        {
                            comando.ExecuteNonQuery();
                        }
                    }

                    MensajeSistema = "Se han borrado todos los registros de la tabla Estudiantes.";
                }
            }
            catch (Exception ex)
            {
                MensajeSistema = "Error al realizar la operación: " + ex.Message;
            }

            // Recarga la lista actualizada de la BD antes de renderizar la página
            CargarEstudiantes();
            return Page();
        }
    }
}