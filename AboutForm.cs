using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace CFAS
{
    /// <summary>Вікно «Про програму»: назва, версія, автори, контакти технічної підтримки.</summary>
    public class AboutForm : Form
    {
        private const string ProgramName = "CFAS";
        private const string Description = "Інформаційна система автоматизації діяльності комп’ютерної фірми";
        private const string Authors = "Острянський Денис Олександрович, група 581-ІПЗ";
        private const string Copyright = "© 2026 Комп’ютерна фірма «Тех-Сервіс»";
        private const string SupportEmail = "support@techservice.example";
        private const string SupportPhone = "+380 44 000-00-00";
        private const string SupportHours = "пн–пт, 9:00–18:00";

        private readonly LinkLabel lnkEmail = new LinkLabel();

        public AboutForm()
        {
            Text = "Про програму";
            Icon = Icons.WindowIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(470, 300);

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var picLogo = new PictureBox { Image = Icons.Logo(), SizeMode = PictureBoxSizeMode.AutoSize, Location = new Point(18, 18) };

            var lblTitle = new Label
            {
                Text = ProgramName,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(66, 14)
            };

            var lblDescription = new Label { Text = Description, Bounds = new Rectangle(66, 48, 390, 36) };
            var lblVersion = new Label { Text = "Версія: " + version, AutoSize = true, Location = new Point(18, 100) };
            var lblAuthors = new Label { Text = "Автори: " + Authors, Bounds = new Rectangle(18, 124, 440, 20) };
            var lblCopyright = new Label { Text = Copyright, AutoSize = true, Location = new Point(18, 148) };

            var lblSupport = new Label
            {
                Text = "Технічна підтримка",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(18, 186)
            };

            var lblEmail = new Label { Text = "E-mail:", AutoSize = true, Location = new Point(18, 210) };
            lnkEmail.Text = SupportEmail;
            lnkEmail.AutoSize = true;
            lnkEmail.Location = new Point(80, 210);
            lnkEmail.LinkClicked += lnkEmail_LinkClicked;

            var lblPhone = new Label { Text = "Телефон: " + SupportPhone + " (" + SupportHours + ")", AutoSize = true, Location = new Point(18, 234) };

            var btnOK = new Button { Text = "OK", Bounds = new Rectangle(370, 256, 85, 28), DialogResult = DialogResult.OK };

            Controls.AddRange(new Control[] {
                picLogo, lblTitle, lblDescription, lblVersion, lblAuthors, lblCopyright,
                lblSupport, lblEmail, lnkEmail, lblPhone, btnOK
            });

            AcceptButton = btnOK;
            CancelButton = btnOK;
        }

        private void lnkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("mailto:" + SupportEmail);
            }
            catch (Exception)
            {
                MessageBox.Show("Не вдалося відкрити поштову програму. Напишіть на адресу " + SupportEmail + ".",
                    "Повідомлення", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
