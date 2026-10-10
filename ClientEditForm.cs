using System;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class ClientEditForm : Form
    {
        private readonly TextBox txtFullName = new TextBox();
        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtAddress = new TextBox();

        public string FullName => txtFullName.Text.Trim();
        public string Phone => txtPhone.Text.Trim();
        public string Email => txtEmail.Text.Trim();
        public string Address => txtAddress.Text.Trim();

        public ClientEditForm() { InitForm("Додавання клієнта"); }

        public ClientEditForm(string name, string phone, string email, string address) : this()
        {
            Text = "Редагування клієнта";
            txtFullName.Text = name;
            txtPhone.Text = phone;
            txtEmail.Text = email;
            txtAddress.Text = address;
        }

        private void InitForm(string title)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(430, 215);

            txtFullName.SetBounds(100, 18, 315, 23);
            txtPhone.SetBounds(100, 52, 315, 23);
            txtEmail.SetBounds(100, 86, 315, 23);
            txtAddress.SetBounds(100, 120, 315, 23);

            var btnOK = new Button { Text = "OK", Bounds = new Rectangle(120, 160, 90, 30) };
            btnOK.Click += (s, e) => {
                if (string.IsNullOrEmpty(FullName))
                {
                    MessageBox.Show("Введіть ПІБ клієнта.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new Button { Text = "Скасувати", Bounds = new Rectangle(220, 160, 105, 30), DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] {
                MakeLabel("ПІБ:", 18), MakeLabel("Телефон:", 52), MakeLabel("Email:", 86), MakeLabel("Адреса:", 120),
                txtFullName, txtPhone, txtEmail, txtAddress, btnOK, btnCancel
            });

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        private Label MakeLabel(string text, int y) => new Label { Text = text, AutoSize = true, Location = new Point(12, y + 3) };
    }
}