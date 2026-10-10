using System;
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

            // Спочатку відкриваємо вікно авторизації
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK)
                {
                    return; // Якщо вхід скасовано — завершуємо роботу програми
                }
            }

            // Якщо авторизація успішна — відкриваємо головне вікно
            Application.Run(new MainForm());
        }
    }
}