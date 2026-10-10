using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CFAS
{
    /// <summary>
    /// Допоміжний клас для виконання SQL-запитів до бази даних.
    /// </summary>
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                var settings = ConfigurationManager.ConnectionStrings["ComputerFirm"];
                if (settings != null && !string.IsNullOrEmpty(settings.ConnectionString))
                {
                    return settings.ConnectionString;
                }
                // Резервний рядок підключення для LocalDB
                return "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ComputerFirm;Integrated Security=True";
            }
        }

        // Виконання SELECT-запитів (повертає таблицю даних)
        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                using (var adapter = new SqlDataAdapter(command))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        // Виконання INSERT, UPDATE, DELETE або збережених процедур
        public static int Exec(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}