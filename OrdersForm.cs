using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class OrdersForm : Form
    {
        private readonly TextBox txtSearch = new TextBox();
        private readonly DataGridView dgvOrders = new DataGridView();
        private DataTable _table;

        public OrdersForm()
        {
            Text = "Замовлення";
            Icon = Icons.WindowIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(850, 450);

            var panel = new Panel { Dock = DockStyle.Top, Height = 42 };
            txtSearch.SetBounds(12, 9, 250, 23);
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            var btnNew = new Button { Text = "Створити", Bounds = new Rectangle(278, 7, 100, 28) };
            btnNew.Click += (s, e) => { using (var f = new OrderEditForm()) if (f.ShowDialog() == DialogResult.OK) LoadOrders(); };

            var btnEdit = new Button { Text = "Відкрити", Bounds = new Rectangle(385, 7, 100, 28) };
            btnEdit.Click += (s, e) => {
                if (dgvOrders.CurrentRow == null) return;
                int id = Convert.ToInt32(((DataRowView)dgvOrders.CurrentRow.DataBoundItem)["order_id"]);
                using (var f = new OrderEditForm(id)) if (f.ShowDialog() == DialogResult.OK) LoadOrders();
            };

            var btnDelete = new Button { Text = "Видалити", Bounds = new Rectangle(492, 7, 100, 28) };
            btnDelete.Click += btnDelete_Click;

            panel.Controls.AddRange(new Control[] { txtSearch, btnNew, btnEdit, btnDelete });

            dgvOrders.Dock = DockStyle.Fill;
            dgvOrders.ReadOnly = true;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.DataBindingComplete += (s, e) => { dgvOrders.Columns["order_id"].Visible = false; };

            Controls.Add(dgvOrders);
            Controls.Add(panel);
            LoadOrders();
        }

        private void LoadOrders()
        {
            _table = Db.Query(
                "SELECT o.order_id, o.order_date AS [Дата], c.full_name AS [Клієнт], ISNULL(u.login, '-') AS [Менеджер], o.status AS [Статус], dbo.fn_order_total(o.order_id) AS [Сума] " +
                "FROM orders o JOIN clients c ON c.client_id = o.client_id LEFT JOIN users u ON u.user_id = o.user_id ORDER BY o.order_date DESC");
            dgvOrders.DataSource = _table;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string text = txtSearch.Text.Trim().Replace("'", "''");
            _table.DefaultView.RowFilter = text.Length == 0 ? "" : "[Клієнт] LIKE '%" + text + "%' OR [Статус] LIKE '%" + text + "%'";
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null) return;
            int id = Convert.ToInt32(((DataRowView)dgvOrders.CurrentRow.DataBoundItem)["order_id"]);

            if (MessageBox.Show("Видалити замовлення № " + id + "?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            Db.Exec("UPDATE p SET p.stock_quantity = p.stock_quantity + s.qty FROM products p JOIN (SELECT product_id, SUM(quantity) AS qty FROM order_items WHERE order_id = @id GROUP BY product_id) s ON s.product_id = p.product_id", new SqlParameter("@id", id));
            Db.Exec("DELETE FROM order_items WHERE order_id = @id", new SqlParameter("@id", id));
            Db.Exec("DELETE FROM orders WHERE order_id = @id", new SqlParameter("@id", id));

            Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text",
                new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", "ORDER"), new SqlParameter("@event_text", "Видалено замовлення № " + id));
            LoadOrders();
        }
    }
}