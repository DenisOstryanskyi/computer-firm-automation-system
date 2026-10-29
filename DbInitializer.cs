using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CFAS
{
    /// <summary>
    /// Створює базу даних при першому запуску програми, якщо її ще немає.
    /// Існуючу базу не змінює.
    /// </summary>
    public static class DbInitializer
    {
        private const string MasterDatabase = "master";
        private const string ScriptRelativePath = @"Database\create_db.sql";
        private const int ConnectTimeoutSeconds = 60;

        /// <summary>
        /// Перевіряє наявність бази; якщо її немає — виконує скрипт створення.
        /// Повертає false, якщо базу створити не вдалося (повідомлення вже показано).
        /// </summary>
        public static bool EnsureCreated()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(Db.ConnectionString);
                string database = builder.InitialCatalog;
                builder.InitialCatalog = MasterDatabase;
                // Перше підключення до LocalDB створює екземпляр, тому таймаут збільшено
                builder.ConnectTimeout = ConnectTimeoutSeconds;

                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    if (DatabaseExists(connection, database))
                    {
                        return true;
                    }

                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScriptRelativePath);
                    if (!File.Exists(path))
                    {
                        MessageBox.Show("Базу даних «" + database + "» не знайдено, а скрипт її створення відсутній:\n" + path,
                            "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    RunScript(connection, File.ReadAllText(path, Encoding.UTF8));
                }

                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не вдалося підключитися до SQL Server LocalDB. Перевірте, що LocalDB встановлено.\n\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Не вдалося прочитати скрипт створення бази даних.\n\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool DatabaseExists(SqlConnection connection, string database)
        {
            using (var command = new SqlCommand("SELECT DB_ID(@name)", connection))
            {
                command.Parameters.AddWithValue("@name", database);
                return !(command.ExecuteScalar() is DBNull);
            }
        }

        // Скрипт ділиться на пакети за рядками GO, як це робить SSMS
        private static void RunScript(SqlConnection connection, string script)
        {
            string[] batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (string batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                using (var command = new SqlCommand(batch, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
