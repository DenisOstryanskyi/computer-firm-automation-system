using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class ClientsForm : Form
    {
        private readonly TextBox txtSearch = new TextBox();
        private readonly DataGridView dgvClients = new DataGridView();
        private DataTable _table;

        public ClientsForm()
        {
            Text = "Довідник клієнтів";
            Icon = Icons.WindowIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(800, 450);

            var panel = new Panel { Dock = DockStyle.Top, Height = 42 };
            txtSearch.SetBounds(12, 9, 250, 23);
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            var btnAdd = new Button { Text = "Додати", Bounds = new Rectangle(278, 7, 95, 28) };
            btnAdd.Click += btnAdd_Click;

            var btnEdit = new Button { Text = "Редагувати", Bounds = new Rectangle(380, 7, 110, 28) };
            btnEdit.Click += btnEdit_Click;

            var btnDelete = new Button { Text = "Видалити", Bounds = new Rectangle(497, 7, 95, 28) };
            btnDelete.Click += btnDelete_Click;

            panel.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnEdit, btnDelete });

            dgvClients.Dock = DockStyle.Fill;
            dgvClients.ReadOnly = true;
            dgvClients.AllowUserToAddRows = false;
            dgvClients.RowHeadersVisible = false;
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(dgvClients);
            Controls.Add(panel);
            LoadClients();
        }

        private void LoadClients()
        {
            _table = Db.Query("SELECT client_id, full_name AS [ПІБ], phone AS [Телефон], email AS [Email], address AS [Адреса] FROM clients ORDER BY full_name");
            dgvClients.DataSource = _table;
            dgvClients.Columns["client_id"].Visible = false;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string text = txtSearch.Text.Trim().Replace("'", "''");
            _table.DefaultView.RowFilter = text.Length == 0 ? "" : "[ПІБ] LIKE '%" + text + "%' OR [Телефон] LIKE '%" + text + "%'";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var edit = new ClientEditForm())
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    Db.Exec("INSERT INTO clients (full_name, phone, email, address) VALUES (@name, @phone, @email, @address)",
                        new SqlParameter("@name", edit.FullName), new SqlParameter("@phone", edit.Phone),
                        new SqlParameter("@email", edit.Email), new SqlParameter("@address", edit.Address));
                    Log("INSERT", "Додано клієнта: " + edit.FullName);
                    LoadClients();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow == null) return;
            var row = (DataRowView)dgvClients.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["client_id"]);

            using (var edit = new ClientEditForm(row["ПІБ"].ToString(), row["Телефон"].ToString(), row["Email"].ToString(), row["Адреса"].ToString()))
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    Db.Exec("UPDATE clients SET full_name = @name, phone = @phone, email = @email, address = @address WHERE client_id = @id",
                        new SqlParameter("@name", edit.FullName), new SqlParameter("@phone", edit.Phone),
                        new SqlParameter("@email", edit.Email), new SqlParameter("@address", edit.Address), new SqlParameter("@id", id));
                    Log("UPDATE", "Відредаговано клієнта: " + edit.FullName);
                    LoadClients();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow == null) return;
            var row = (DataRowView)dgvClients.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["client_id"]);
            string name = row["ПІБ"].ToString();

            if (MessageBox.Show("Видалити клієнта «" + name + "»?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                Db.Exec("DELETE FROM clients WHERE client_id = @id", new SqlParameter("@id", id));
                Log("DELETE", "Видалено клієнта: " + name);
                LoadClients();
            }
            catch (SqlException)
            {
                MessageBox.Show("Неможливо видалити клієнта, оскільки у нього є замовлення.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string type, string text)
        {
            Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text",
                new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", type), new SqlParameter("@event_text", text));
        }
    }
}