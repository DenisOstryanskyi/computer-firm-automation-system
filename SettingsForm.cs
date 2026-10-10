using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class SettingsForm : Form
    {
        private readonly TextBox txtValue = new TextBox();
        private readonly DataGridView dgvSettings = new DataGridView();
        private DataTable _table;

        public SettingsForm()
        {
            Text = "Системні параметри";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(650, 300);

            var panel = new Panel { Dock = DockStyle.Top, Height = 42 };
            txtValue.SetBounds(12, 9, 350, 23);

            var btnApply = new Button { Text = "Застосувати", Bounds = new Rectangle(375, 7, 110, 28) };
            btnApply.Click += btnApply_Click;

            panel.Controls.AddRange(new Control[] { txtValue, btnApply });

            dgvSettings.Dock = DockStyle.Fill;
            dgvSettings.ReadOnly = true;
            dgvSettings.AllowUserToAddRows = false;
            dgvSettings.RowHeadersVisible = false;
            dgvSettings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSettings.SelectionChanged += (s, e) => {
                if (dgvSettings.CurrentRow != null)
                    txtValue.Text = ((DataRowView)dgvSettings.CurrentRow.DataBoundItem)["Значення"].ToString();
            };

            Controls.Add(dgvSettings);
            Controls.Add(panel);
            LoadSettings();
        }

        private void LoadSettings()
        {
            _table = Db.Query("SELECT setting_key AS [Параметр], setting_value AS [Значення], description AS [Опис] FROM settings ORDER BY setting_key");
            dgvSettings.DataSource = _table;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (dgvSettings.CurrentRow == null) return;
            string key = ((DataRowView)dgvSettings.CurrentRow.DataBoundItem)["Параметр"].ToString();
            Db.Exec("UPDATE settings SET setting_value = @value WHERE setting_key = @key",
                new SqlParameter("@value", txtValue.Text.Trim()), new SqlParameter("@key", key));
            Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text", new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", "SYSTEM"), new SqlParameter("@event_text", "Змінено параметр: " + key));
            LoadSettings();
        }
    }
}