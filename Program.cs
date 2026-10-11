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