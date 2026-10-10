using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    /// <summary>
    /// Журнал подій системи (лише для адміністратора).
    /// </summary>
    public class JournalForm : Form
    {
        private readonly DataGridView dgvJournal = new DataGridView();
        private readonly ComboBox cmbEventType = new ComboBox();
        private readonly DateTimePicker dtpDate = new DateTimePicker();
        private DataTable _table;

        public JournalForm()
        {
            Text = "Журнал подій системи";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(850, 480);

            var panel = new Panel { Dock = DockStyle.Top, Height = 42 };

            var lblType = new Label { Text = "Тип:", AutoSize = true, Location = new Point(12, 14) };
            cmbEventType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEventType.SetBounds(45, 10, 130, 23);
            cmbEventType.Items.Add("Всі");
            cmbEventType.Items.AddRange(new object[] { "LOGIN", "LOGIN_FAIL", "INSERT", "UPDATE", "DELETE", "ORDER", "SYSTEM" });
            cmbEventType.SelectedIndex = 0;
            cmbEventType.SelectedIndexChanged += delegate { ApplyFilter(); };

            var lblDate = new Label { Text = "Дата від:", AutoSize = true, Location = new Point(190, 14) };
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.SetBounds(250, 10, 100, 23);
            dtpDate.Value = DateTime.Today.AddMonths(-1);
            dtpDate.ValueChanged += delegate { LoadJournal(); };

            var btnRefresh = new Button { Text = "Оновити" };
            btnRefresh.SetBounds(365, 8, 90, 28);
            btnRefresh.Click += delegate { LoadJournal(); };

            panel.Controls.Add(lblType);
            panel.Controls.Add(cmbEventType);
            panel.Controls.Add(lblDate);
            panel.Controls.Add(dtpDate);
            panel.Controls.Add(btnRefresh);

            dgvJournal.Dock = DockStyle.Fill;
            dgvJournal.ReadOnly = true;
            dgvJournal.AllowUserToAddRows = false;
            dgvJournal.RowHeadersVisible = false;
            dgvJournal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvJournal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvJournal.MultiSelect = false;

            Controls.Add(dgvJournal);
            Controls.Add(panel);

            LoadJournal();
        }

        private void LoadJournal()
        {
            _table = Db.Query(
                "SELECT e.event_id, e.event_time AS [Час], ISNULL(u.login, N'Система') AS [Користувач], " +
                "e.event_type AS [Тип], e.event_text AS [Опис] " +
                "FROM event_log e LEFT JOIN users u ON u.user_id = e.user_id " +
                "WHERE e.event_time >= @date " +
                "ORDER BY e.event_time DESC",
                new SqlParameter("@date", dtpDate.Value.Date));
            dgvJournal.DataSource = _table;
            dgvJournal.Columns["event_id"].Visible = false;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_table == null) return;
            string type = cmbEventType.SelectedItem?.ToString();
            if (type == null || type == "Всі")
            {
                _table.DefaultView.RowFilter = "";
            }
            else
            {
                _table.DefaultView.RowFilter = "[Тип] = '" + type + "'";
            }
        }
    }
}