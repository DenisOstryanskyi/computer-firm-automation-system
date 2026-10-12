using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class OrderEditForm : Form
    {
        private readonly ComboBox cmbClient = new ComboBox();
        private readonly ComboBox cmbStatus = new ComboBox();
        private readonly TextBox txtComment = new TextBox();
        private readonly ComboBox cmbProduct = new ComboBox();
        private readonly TextBox txtQuantity = new TextBox();
        private readonly TextBox txtPrice = new TextBox();
        private readonly Label lblStock = new Label();
        private readonly Label lblTotal = new Label();
        private readonly DataGridView dgvItems = new DataGridView();
        private readonly DataTable _items = new DataTable();
        private readonly int _orderId;

        public OrderEditForm() : this(0) { }

        public OrderEditForm(int orderId)
        {
            _orderId = orderId;
            Text = orderId == 0 ? "Нове замовлення" : "Замовлення № " + orderId;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 500);
            Icon = Icons.WindowIcon();

            _items.Columns.Add("product_id", typeof(int));
            _items.Columns.Add("Товар", typeof(string));
            _items.Columns.Add("Кількість", typeof(int));
            _items.Columns.Add("Ціна", typeof(decimal));
            _items.Columns.Add("Сума", typeof(decimal), "Кількість * Ціна");

            var panelHeader = new Panel { Dock = DockStyle.Top, Height = 40 };

            cmbClient.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbClient.SetBounds(75, 9, 200, 23);
            DataTable clients = Db.Query("SELECT client_id, full_name FROM clients ORDER BY full_name");
            cmbClient.DataSource = clients;
            cmbClient.DisplayMember = "full_name";
            cmbClient.ValueMember = "client_id";

            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.SetBounds(345, 9, 110, 23);
            cmbStatus.Items.AddRange(new object[] { "Нове", "Виконується", "Виконане", "Скасоване" });
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;

            txtComment.SetBounds(540, 9, 205, 23);

            panelHeader.Controls.AddRange(new Control[] {
                new Label { Text = "Клієнт:", AutoSize = true, Location = new Point(12, 12) }, cmbClient,
                new Label { Text = "Статус:", AutoSize = true, Location = new Point(285, 12) }, cmbStatus,
                new Label { Text = "Коментар:", AutoSize = true, Location = new Point(465, 12) }, txtComment
            });

            var panelItems = new Panel { Dock = DockStyle.Top, Height = 40 };

            cmbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduct.SetBounds(62, 9, 160, 23);
            DataTable products = Db.Query("SELECT product_id, product_name, price, stock_quantity FROM products ORDER BY product_name");
            cmbProduct.DataSource = products;
            cmbProduct.DisplayMember = "product_name";
            cmbProduct.ValueMember = "product_id";

            txtQuantity.SetBounds(282, 9, 40, 23);
            txtQuantity.Text = "1";
            txtPrice.SetBounds(330, 9, 70, 23);
            txtPrice.ReadOnly = true;

            lblStock.AutoSize = true;
            lblStock.Location = new Point(410, 12);

            cmbProduct.SelectedIndexChanged += (s, e) => ShowProductInfo();
            ShowProductInfo();

            var btnAddItem = new Button { Text = "Додати", Bounds = new Rectangle(530, 7, 95, 28) };
            btnAddItem.Click += btnAddItem_Click;

            var btnRemoveItem = new Button { Text = "Прибрати", Bounds = new Rectangle(632, 7, 110, 28) };
            btnRemoveItem.Click += (s, e) => {
                if (dgvItems.CurrentRow != null)
                {
                    _items.Rows.Remove(((DataRowView)dgvItems.CurrentRow.DataBoundItem).Row);
                    UpdateTotal();
                }
            };

            panelItems.Controls.AddRange(new Control[] {
                new Label { Text = "Товар:", AutoSize = true, Location = new Point(12, 12) }, cmbProduct,
                new Label { Text = "К-сть:", AutoSize = true, Location = new Point(230, 12) }, txtQuantity, txtPrice, lblStock, btnAddItem, btnRemoveItem
            });

            dgvItems.Dock = DockStyle.Fill;
            dgvItems.ReadOnly = true;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.RowHeadersVisible = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.DataBindingComplete += (s, e) => { if (dgvItems.Columns["product_id"] != null) dgvItems.Columns["product_id"].Visible = false; };
            dgvItems.DataSource = _items;

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 42 };
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(12, 12);

            var btnOK = new Button { Text = "Зберегти", Bounds = new Rectangle(540, 7, 105, 28) };
            btnOK.Click += btnOK_Click;

            var btnCancel = new Button { Text = "Скасувати", Bounds = new Rectangle(650, 7, 95, 28), DialogResult = DialogResult.Cancel };

            panelBottom.Controls.AddRange(new Control[] { lblTotal, btnOK, btnCancel });

            Controls.Add(dgvItems);
            Controls.Add(panelBottom);
            Controls.Add(panelItems);
            Controls.Add(panelHeader);

            AcceptButton = btnOK;
            CancelButton = btnCancel;

            if (orderId != 0) LoadOrder(orderId);
            UpdateTotal();
        }

        // Показує ціну та залишок вибраного товару
        private void ShowProductInfo()
        {
            if (cmbProduct.SelectedItem is DataRowView r)
            {
                txtPrice.Text = Convert.ToDecimal(r["price"]).ToString("0.00");
                lblStock.Text = "На складі: " + r["stock_quantity"];
            }
        }

        private void LoadOrder(int orderId)
        {
            var order = Db.Query("SELECT client_id, status, comment FROM orders WHERE order_id = @id", new SqlParameter("@id", orderId));
            if (order.Rows.Count > 0)
            {
                if (order.Rows[0]["client_id"] != DBNull.Value) cmbClient.SelectedValue = order.Rows[0]["client_id"];
                if (order.Rows[0]["status"] != DBNull.Value) cmbStatus.Text = order.Rows[0]["status"].ToString();
                if (order.Rows[0]["comment"] != DBNull.Value) txtComment.Text = order.Rows[0]["comment"].ToString();
            }
            foreach (DataRow r in Db.Query("SELECT oi.product_id, p.product_name, oi.quantity, oi.unit_price FROM order_items oi JOIN products p ON p.product_id = oi.product_id WHERE oi.order_id = @id", new SqlParameter("@id", orderId)).Rows)
            {
                _items.Rows.Add(r["product_id"], r["product_name"], r["quantity"], r["unit_price"]);
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataRow r in _items.Rows) total += Convert.ToDecimal(r["Сума"]);
            lblTotal.Text = "Разом: " + total.ToString("N2") + " грн";
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (!(cmbProduct.SelectedItem is DataRowView prod) || !int.TryParse(txtQuantity.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Виберіть товар та введіть коректну кількість.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int prodId = Convert.ToInt32(prod["product_id"]);
            int stock = Convert.ToInt32(prod["stock_quantity"]);

            int already = 0;
            foreach (DataRow item in _items.Rows)
                if (Convert.ToInt32(item["product_id"]) == prodId) already += Convert.ToInt32(item["Кількість"]);

            if (already + qty > stock)
            {
                MessageBox.Show("Недостатньо товару на складі (є: " + stock + ").", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _items.Rows.Add(prodId, prod["product_name"], qty, Convert.ToDecimal(prod["price"]));
            UpdateTotal();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cmbClient.SelectedValue == null) { MessageBox.Show("Виберіть клієнта.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int clientId = Convert.ToInt32(cmbClient.SelectedValue);
            string status = cmbStatus.Text;
            string comment = txtComment.Text.Trim();

            try
            {
                if (_orderId == 0)
                {
                    Db.Exec("INSERT INTO orders (client_id, user_id, status, comment) VALUES (@client, @user, @status, @comment)",
                        new SqlParameter("@client", clientId), new SqlParameter("@user", Session.UserId), new SqlParameter("@status", status), new SqlParameter("@comment", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment));
                    int newId = Convert.ToInt32(Db.Query("SELECT MAX(order_id) FROM orders").Rows[0][0]);
                    InsertItems(newId);
                    Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text", new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", "ORDER"), new SqlParameter("@event_text", "Створено замовлення № " + newId));
                }
                else
                {
                    Db.Exec("UPDATE orders SET client_id = @client, status = @status, comment = @comment WHERE order_id = @id",
                        new SqlParameter("@client", clientId), new SqlParameter("@status", status), new SqlParameter("@comment", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment), new SqlParameter("@id", _orderId));

                    Db.Exec("UPDATE p SET p.stock_quantity = p.stock_quantity + s.qty FROM products p JOIN (SELECT product_id, SUM(quantity) AS qty FROM order_items WHERE order_id = @id GROUP BY product_id) s ON s.product_id = p.product_id", new SqlParameter("@id", _orderId));
                    Db.Exec("DELETE FROM order_items WHERE order_id = @id", new SqlParameter("@id", _orderId));
                    InsertItems(_orderId);
                    Db.Exec("EXEC sp_log_event @user_id, @event_type, @event_text", new SqlParameter("@user_id", Session.UserId), new SqlParameter("@event_type", "ORDER"), new SqlParameter("@event_text", "Змінено замовлення № " + _orderId));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Помилка збереження замовлення.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertItems(int orderId)
        {
            foreach (DataRow item in _items.Rows)
            {
                Db.Exec("INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES (@order, @product, @qty, @price)",
                    new SqlParameter("@order", orderId), new SqlParameter("@product", item["product_id"]), new SqlParameter("@qty", item["Кількість"]), new SqlParameter("@price", item["Ціна"]));
            }
        }
    }
}