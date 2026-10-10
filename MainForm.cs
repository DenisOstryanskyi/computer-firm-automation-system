using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace CFAS
{
    public class MainForm : Form
    {
        private ToolStripMenuItem _miJournal;
        private ToolStripMenuItem _miUsers;
        private ToolStripMenuItem _miSettings;

        public MainForm()
        {
            BuildMenu();
            Text = "CFAS — Комп'ютерна фірма — " + Session.FullName + " (" + Session.RoleName + ")";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(850, 500);

            // Адміністративні функції приховані для звичайних користувачів
            if (Session.RoleName != "Адміністратор")
            {
                _miJournal.Visible = false;
                _miUsers.Visible = false;
                _miSettings.Visible = false;
            }
        }

        private void BuildMenu()
        {
            var miDirectories = new ToolStripMenuItem("Довідники");
            var miClients = new ToolStripMenuItem("Клієнти");
            miClients.Click += (s, e) => new ClientsForm().ShowDialog();
            var miProducts = new ToolStripMenuItem("Товари");
            miProducts.Click += (s, e) => new ProductsForm().ShowDialog();
            miDirectories.DropDownItems.AddRange(new ToolStripItem[] { miClients, miProducts });

            var miOperations = new ToolStripMenuItem("Операції");
            var miOrders = new ToolStripMenuItem("Замовлення");
            miOrders.Click += (s, e) => new OrdersForm().ShowDialog();
            miOperations.DropDownItems.Add(miOrders);

            var miReportsMenu = new ToolStripMenuItem("Звіти");
            var miReports = new ToolStripMenuItem("Звіт продажів");
            miReports.Click += (s, e) => new ReportsForm().ShowDialog();
            miReportsMenu.DropDownItems.Add(miReports);

            var miService = new ToolStripMenuItem("Сервіс");
            _miJournal = new ToolStripMenuItem("Журнал подій");
            _miJournal.Click += (s, e) => new JournalForm().ShowDialog();
            _miUsers = new ToolStripMenuItem("Користувачі");
            _miUsers.Click += (s, e) => new UsersForm().ShowDialog();
            _miSettings = new ToolStripMenuItem("Налаштування");
            _miSettings.Click += (s, e) => new SettingsForm().ShowDialog();
            miService.DropDownItems.AddRange(new ToolStripItem[] { _miJournal, _miUsers, _miSettings });

            var miHelpMenu = new ToolStripMenuItem("Довідка");
            var miHelp = new ToolStripMenuItem("Довідка");
            miHelp.Click += (s, e) => new HelpForm().ShowDialog();
            var miAbout = new ToolStripMenuItem("Про програму");
            miAbout.Click += miAbout_Click;
            miHelpMenu.DropDownItems.AddRange(new ToolStripItem[] { miHelp, miAbout });

            var menu = new MenuStrip();
            menu.Items.AddRange(new ToolStripItem[] { miDirectories, miOperations, miReportsMenu, miService, miHelpMenu });
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text",
                new SqlParameter("@user_id", Session.UserId),
                new SqlParameter("@event_type", "LOGOUT"),
                new SqlParameter("@event_text", "Вихід із системи"));
            base.OnFormClosed(e);
        }

        private void miAbout_Click(object sender, System.EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show(
                "CFAS — інформаційна система автоматизації діяльності комп'ютерної фірми.\n" +
                "Версія: " + version + "\n" +
                "Розробник: Острянський Денис, група 581-ІПЗ",
                "Про програму", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}