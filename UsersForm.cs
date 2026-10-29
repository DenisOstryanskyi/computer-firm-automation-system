using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class UsersForm : Form
    {
        private readonly TextBox txtSearch = new TextBox();
        private readonly DataGridView dgvUsers = new DataGridView();
        private DataTable _table;

        public UsersForm()
        {
            Text = "Користувачі системи";
            Icon = Icons.WindowIcon();
            HelpForm.AttachF1(this, HelpForm.TopicUsers);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(700, 420);

            var panel = new Panel { Dock = DockStyle.Top, Height = 42 };
            txtSearch.SetBounds(12, 9, 250, 23);
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            var btnAdd = new Button { Text = "Додати", Bounds = new Rectangle(278, 7, 95, 28) };
            btnAdd.Click += btnAdd_Click;

            var btnEdit = new Button { Text = "Редагувати", Bounds = new Rectangle(380, 7, 110, 28) };
            btnEdit.Click += btnEdit_Click;

            panel.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnEdit });

            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.ReadOnly = true;
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.RowHeadersVisible = false;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.DataBindingComplete += (s, e) => { dgvUsers.Columns["user_id"].Visible = false; };

            Controls.Add(dgvUsers);
            Controls.Add(panel);
            LoadUsers();
        }

        private void LoadUsers()
        {
            _table = Db.Query("SELECT u.user_id, u.login AS [Логін], u.full_name AS [ПІБ], r.role_name AS [Роль], u.is_active AS [Активний] FROM users u JOIN roles r ON r.role_id = u.role_id ORDER BY u.login");
            dgvUsers.DataSource = _table;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string text = txtSearch.Text.Trim().Replace("'", "''");
            _table.DefaultView.RowFilter = text.Length == 0 ? "" : "[Логін] LIKE '%" + text + "%' OR [ПІБ] LIKE '%" + text + "%'";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var edit = new UsersEditForm())
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Db.Exec("INSERT INTO users (login, password_hash, full_name, role_id, is_active) VALUES (@login, @hash, @name, @role, @active)",
                            new SqlParameter("@login", edit.UserLogin), new SqlParameter("@hash", PasswordHasher.Hash(edit.Password)),
                            new SqlParameter("@name", edit.UserFullName), new SqlParameter("@role", edit.RoleId), new SqlParameter("@active", edit.IsActive));
                        Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text", new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", "INSERT"), new SqlParameter("@event_text", "Додано користувача: " + edit.UserLogin));
                        LoadUsers();
                    }
                    catch (SqlException)
                    {
                        MessageBox.Show("Користувач з таким логіном уже існує.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null) return;
            var row = (DataRowView)dgvUsers.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["user_id"]);

            using (var edit = new UsersEditForm(row["Логін"].ToString(), row["ПІБ"].ToString(), row["Роль"].ToString(), Convert.ToBoolean(row["Активний"])))
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(edit.Password))
                    {
                        Db.Exec("UPDATE users SET full_name = @name, role_id = @role, is_active = @active WHERE user_id = @id",
                            new SqlParameter("@name", edit.UserFullName), new SqlParameter("@role", edit.RoleId), new SqlParameter("@active", edit.IsActive), new SqlParameter("@id", id));
                    }
                    else
                    {
                        Db.Exec("UPDATE users SET password_hash = @hash, full_name = @name, role_id = @role, is_active = @active WHERE user_id = @id",
                            new SqlParameter("@hash", PasswordHasher.Hash(edit.Password)), new SqlParameter("@name", edit.UserFullName), new SqlParameter("@role", edit.RoleId), new SqlParameter("@active", edit.IsActive), new SqlParameter("@id", id));
                    }
                    Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text", new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", "UPDATE"), new SqlParameter("@event_text", "Відредаговано користувача: " + edit.UserLogin));
                    LoadUsers();
                }
            }
        }
    }
}