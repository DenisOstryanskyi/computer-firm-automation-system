using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CFAS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                string msg = e.Exception is SqlException
                    ? "Не вдалося підключитися до бази даних. Перевірте налаштування підключення."
                    : "Сталася внутрішня помилка: " + e.Exception.Message;
                MessageBox.Show(msg, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            if (!DbInitializer.EnsureCreated())
            {
                return;
            }

            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            Application.Run(new MainForm());
        }
    }
}