using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class ProductsForm : Form
    {
        private readonly TextBox txtSearch = new TextBox();
        private readonly DataGridView dgvProducts = new DataGridView();
        private DataTable _table;

        public ProductsForm()
        {
            Text = "Довідник товарів";
            Icon = Icons.WindowIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(820, 450);

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

            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.DataBindingComplete += (s, e) => { dgvProducts.Columns["product_id"].Visible = false; };

            Controls.Add(dgvProducts);
            Controls.Add(panel);
            LoadProducts();
        }

        private void LoadProducts()
        {
            _table = Db.Query("SELECT p.product_id, p.product_name AS [Назва], c.category_name AS [Категорія], p.price AS [Ціна], p.stock_quantity AS [Залишок] FROM products p JOIN categories c ON c.category_id = p.category_id ORDER BY p.product_name");
            dgvProducts.DataSource = _table;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string text = txtSearch.Text.Trim().Replace("'", "''");
            _table.DefaultView.RowFilter = text.Length == 0 ? "" : "[Назва] LIKE '%" + text + "%' OR [Категорія] LIKE '%" + text + "%'";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var edit = new ProductEditForm())
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    Db.Exec("INSERT INTO products (product_name, category_id, price, stock_quantity) VALUES (@name, @cat, @price, @stock)",
                        new SqlParameter("@name", edit.ProductName), new SqlParameter("@cat", edit.CategoryId),
                        new SqlParameter("@price", edit.Price), new SqlParameter("@stock", edit.StockQuantity));
                    Log("INSERT", "Додано товар: " + edit.ProductName);
                    LoadProducts();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            var row = (DataRowView)dgvProducts.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["product_id"]);

            DataTable pInfo = Db.Query("SELECT category_id, price, stock_quantity FROM products WHERE product_id = @id", new SqlParameter("@id", id));
            if (pInfo.Rows.Count == 0) return;

            using (var edit = new ProductEditForm(row["Назва"].ToString(), Convert.ToInt32(pInfo.Rows[0]["category_id"]), Convert.ToDecimal(pInfo.Rows[0]["price"]), Convert.ToInt32(pInfo.Rows[0]["stock_quantity"])))
            {
                if (edit.ShowDialog() == DialogResult.OK)
                {
                    Db.Exec("UPDATE products SET product_name = @name, category_id = @cat, price = @price, stock_quantity = @stock WHERE product_id = @id",
                        new SqlParameter("@name", edit.ProductName), new SqlParameter("@cat", edit.CategoryId),
                        new SqlParameter("@price", edit.Price), new SqlParameter("@stock", edit.StockQuantity), new SqlParameter("@id", id));
                    Log("UPDATE", "Відредаговано товар: " + edit.ProductName);
                    LoadProducts();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            var row = (DataRowView)dgvProducts.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["product_id"]);
            string name = row["Назва"].ToString();

            if (MessageBox.Show("Видалити товар «" + name + "»?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                Db.Exec("DELETE FROM products WHERE product_id = @id", new SqlParameter("@id", id));
                Log("DELETE", "Видалено товар: " + name);
                LoadProducts();
            }
            catch (SqlException)
            {
                MessageBox.Show("Неможливо видалити товар, оскільки він використовується в замовленнях.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string type, string text)
        {
            Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text",
                new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", type), new SqlParameter("@event_text", text));
        }
    }
}