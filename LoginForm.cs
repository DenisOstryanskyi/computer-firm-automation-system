using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class LoginForm : Form
    {
        private readonly TextBox txtLogin = new TextBox();
        private readonly TextBox txtPassword = new TextBox();

        public LoginForm()
        {
            Text = "Вхід у систему CFAS";
            Icon = Icons.WindowIcon();
            HelpForm.AttachF1(this, HelpForm.TopicLogin);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(330, 175);

            var lblLogin = new Label { Text = "Логін:", AutoSize = true, Location = new Point(12, 18) };
            var lblPassword = new Label { Text = "Пароль:", AutoSize = true, Location = new Point(12, 52) };

            txtLogin.SetBounds(90, 15, 210, 23);
            txtPassword.SetBounds(90, 49, 210, 23);
            txtPassword.PasswordChar = '*';

            var btnLogin = new Button { Text = "Увійти", Bounds = new Rectangle(90, 100, 95, 30) };
            btnLogin.Click += btnLogin_Click;

            var btnExit = new Button { Text = "Вихід", Bounds = new Rectangle(195, 100, 105, 30) };
            btnExit.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { lblLogin, lblPassword, txtLogin, txtPassword, btnLogin, btnExit });
            AcceptButton = btnLogin;
            CancelButton = btnExit;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (login.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Введіть логін і пароль.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable table = Db.Query(
                "SELECT u.user_id, u.full_name, u.login, r.role_name FROM users u " +
                "JOIN roles r ON r.role_id = u.role_id " +
                "WHERE u.login = @login AND u.password_hash = @hash AND u.is_active = 1",
                new SqlParameter("@login", login),
                new SqlParameter("@hash", PasswordHasher.Hash(password)));

            if (table.Rows.Count == 0)
            {
                Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text",
                    new SqlParameter("@user_id", DBNull.Value),
                    new SqlParameter("@event_type", "LOGIN_FAIL"),
                    new SqlParameter("@event_text", "Невдалий вхід: логін '" + login + "'"));
                MessageBox.Show("Невірний логін або пароль.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Session.UserId = Convert.ToInt32(table.Rows[0]["user_id"]);
            Session.Login = table.Rows[0]["login"].ToString();
            Session.FullName = table.Rows[0]["full_name"].ToString();
            Session.RoleName = table.Rows[0]["role_name"].ToString();

            Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text",
                new SqlParameter("@user_id", Session.UserId),
                new SqlParameter("@event_type", "LOGIN"),
                new SqlParameter("@event_text", "Вхід у систему"));

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}