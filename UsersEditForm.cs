using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class UsersEditForm : Form
    {
        private readonly TextBox txtLogin = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtFullName = new TextBox();
        private readonly ComboBox cmbRole = new ComboBox();
        private readonly CheckBox chkActive = new CheckBox();
        private readonly bool _isAdd;

        public string UserLogin => txtLogin.Text.Trim();
        public string UserFullName => txtFullName.Text.Trim();
        public string Password => txtPassword.Text;
        public int RoleId => Convert.ToInt32(cmbRole.SelectedValue);
        public bool IsActive => chkActive.Checked;

        public UsersEditForm()
        {
            _isAdd = true;
            InitForm("Додавання користувача");
            chkActive.Checked = true;
        }

        public UsersEditForm(string login, string name, string roleName, bool active) : this()
        {
            _isAdd = false;
            Text = "Редагування користувача";
            txtLogin.Text = login;
            txtLogin.ReadOnly = true;
            txtFullName.Text = name;
            cmbRole.Text = roleName;
            chkActive.Checked = active;
        }

        private void InitForm(string title)
        {
            Text = title;
            Icon = Icons.WindowIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(430, 250);

            txtLogin.SetBounds(100, 18, 315, 23);
            txtPassword.SetBounds(100, 52, 315, 23);
            txtFullName.SetBounds(100, 86, 315, 23);

            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.SetBounds(100, 120, 315, 23);
            cmbRole.DataSource = Db.Query("SELECT role_id, role_name FROM roles ORDER BY role_name");
            cmbRole.DisplayMember = "role_name";
            cmbRole.ValueMember = "role_id";

            chkActive.Text = "Активний";
            chkActive.AutoSize = true;
            chkActive.Location = new Point(100, 152);

            var btnOK = new Button { Text = "OK", Bounds = new Rectangle(120, 190, 90, 30) };
            btnOK.Click += (s, e) => {
                if (string.IsNullOrEmpty(UserLogin) || string.IsNullOrEmpty(UserFullName) || (_isAdd && string.IsNullOrEmpty(Password)))
                {
                    MessageBox.Show("Заповніть обов'язкові поля.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new Button { Text = "Скасувати", Bounds = new Rectangle(220, 190, 105, 30), DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] {
                MakeLabel("Логін:", 18), MakeLabel("Пароль:", 52), MakeLabel("ПІБ:", 86), MakeLabel("Роль:", 120),
                txtLogin, txtPassword, txtFullName, cmbRole, chkActive, btnOK, btnCancel
            });

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        private Label MakeLabel(string text, int y) => new Label { Text = text, AutoSize = true, Location = new Point(12, y + 3) };
    }
}