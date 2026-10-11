using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CFAS
{
    public class ProductEditForm : Form
    {
        private readonly TextBox txtName = new TextBox();
        private readonly ComboBox cmbCategory = new ComboBox();
        private readonly TextBox txtPrice = new TextBox();
        private readonly TextBox txtStock = new TextBox();

        public string ProductName => txtName.Text.Trim();
        public int CategoryId => Convert.ToInt32(cmbCategory.SelectedValue);
        public decimal Price { get { return ParsePrice(); } }
        public int StockQuantity { get { return ParseStock(); } }

        public ProductEditForm() { InitForm("Додавання товару"); }

        public ProductEditForm(string name, int catId, decimal price, int stock) : this()
        {
            Text = "Редагування товару";
            txtName.Text = name;
            cmbCategory.SelectedValue = catId;
            txtPrice.Text = price.ToString("0.00", CultureInfo.InvariantCulture);
            txtStock.Text = stock.ToString();
        }

              private decimal ParsePrice()
        {
            string text = txtPrice.Text.Trim().Replace(',', '.');
            decimal p;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out p))
            {
                return p;
            }
            return -1;
        }

        private int ParseStock()
        {
            int s;
            if (int.TryParse(txtStock.Text.Trim(), out s))
            {
                return s;
            }
            return -1; 
        }

        private void InitForm(string title)
        {
            Text = title;
            Icon = Icons.WindowIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(430, 230);

            txtName.SetBounds(110, 18, 300, 23);

            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.SetBounds(110, 52, 300, 23);
            cmbCategory.DataSource = Db.Query("SELECT category_id, category_name FROM categories ORDER BY category_name");
            cmbCategory.DisplayMember = "category_name";
            cmbCategory.ValueMember = "category_id";

            txtPrice.SetBounds(110, 86, 300, 23);
            txtStock.SetBounds(110, 120, 300, 23);

            var btnOK = new Button { Text = "OK", Bounds = new Rectangle(120, 170, 90, 30) };
            btnOK.Click += (s, e) => {
                if (string.IsNullOrEmpty(ProductName) || Price < 0 || StockQuantity < 0)
                {
                    MessageBox.Show("Перевірте коректність заповнення полів.", "Попередження",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new Button { Text = "Скасувати", Bounds = new Rectangle(220, 170, 105, 30), DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] {
                MakeLabel("Назва:", 18), MakeLabel("Категорія:", 52), MakeLabel("Ціна (грн):", 86), MakeLabel("Залишок:", 120),
                txtName, cmbCategory, txtPrice, txtStock, btnOK, btnCancel
            });

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        private Label MakeLabel(string text, int y) => new Label { Text = text, AutoSize = true, Location = new Point(12, y + 3) };
    }
}