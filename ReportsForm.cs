using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CFAS
{
    public class ReportsForm : Form
    {
        private readonly DateTimePicker dtpFrom = new DateTimePicker();
        private readonly DateTimePicker dtpTo = new DateTimePicker();
        private readonly Label lblSummary = new Label();
        private readonly DataGridView dgvReport = new DataGridView();
        private readonly Chart chartMonth = new Chart();
        private readonly Chart chartProducts = new Chart();
        private readonly PrintDocument printDocument = new PrintDocument();
        private DataTable _table;

        public ReportsForm()
        {
            Text = "Звіт продажів";
            Icon = Icons.WindowIcon();
            HelpForm.AttachF1(this, HelpForm.TopicReports);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1000, 620);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 42 };
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.SetBounds(80, 9, 105, 23);
            dtpFrom.Value = new DateTime(DateTime.Today.Year, 1, 1);

            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.SetBounds(215, 9, 105, 23);
            dtpTo.Value = DateTime.Today;

            var btnBuild = new Button { Text = "Побудувати", Bounds = new Rectangle(330, 7, 110, 28) };
            btnBuild.Click += (s, e) => BuildReport();

            var btnPrint = new Button { Text = "Друк", Bounds = new Rectangle(450, 7, 80, 28) };
            btnPrint.Click += (s, e) => { new PrintPreviewDialog { Document = printDocument }.ShowDialog(this); };

            lblSummary.AutoSize = true;
            lblSummary.Location = new Point(550, 12);

            panelTop.Controls.AddRange(new Control[] {
                new Label { Text = "Період з:", AutoSize = true, Location = new Point(12, 12) }, dtpFrom,
                new Label { Text = "по:", AutoSize = true, Location = new Point(195, 12) }, dtpTo,
                btnBuild, btnPrint, lblSummary
            });

            dgvReport.Dock = DockStyle.Fill;
            dgvReport.ReadOnly = true;
            dgvReport.AllowUserToAddRows = false;
            dgvReport.RowHeadersVisible = false;
            dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            chartMonth.Dock = DockStyle.Fill;
            chartMonth.ChartAreas.Add(new ChartArea());
            chartMonth.Series.Add(new Series("Продажі") { ChartType = SeriesChartType.Column });
            chartMonth.Titles.Add("Продажі за місяцями, грн");

            chartProducts.Dock = DockStyle.Fill;
            chartProducts.ChartAreas.Add(new ChartArea());
            chartProducts.Series.Add(new Series("Товари") { ChartType = SeriesChartType.Pie });
            chartProducts.Legends.Add(new Legend());
            chartProducts.Titles.Add("Продажі за товарами");

            var chartPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            chartPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            chartPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            chartPanel.Controls.Add(chartMonth, 0, 0);
            chartPanel.Controls.Add(chartProducts, 0, 1);

            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.Controls.Add(dgvReport, 0, 0);
            mainPanel.Controls.Add(chartPanel, 1, 0);

            Controls.Add(mainPanel);
            Controls.Add(panelTop);

            printDocument.PrintPage += printDocument_PrintPage;
            BuildReport();
        }

        private void BuildReport()
        {
        
            _table = Db.Query("EXEC sp_sales_report @from, @to",
                new SqlParameter("@from", dtpFrom.Value.Date), new SqlParameter("@to", dtpTo.Value.Date));

            _table.Columns["order_id"].ColumnName = "№";
            _table.Columns["order_date"].ColumnName = "Дата";
            _table.Columns["client_name"].ColumnName = "Клієнт";
            _table.Columns["order_total"].ColumnName = "Сума";
            dgvReport.DataSource = _table;

            decimal total = 0;
            foreach (DataRow r in _table.Rows) total += Convert.ToDecimal(r["Сума"]);
            lblSummary.Text = "Замовлень: " + _table.Rows.Count + ", на суму: " + total.ToString("N2") + " грн";

            // Діаграма 1: місяці
            chartMonth.Series[0].Points.Clear();
            foreach (DataRow r in Db.Query("SELECT FORMAT(order_date, 'yyyy-MM') AS month_label, SUM(dbo.fn_order_total(order_id)) AS total FROM orders WHERE status = N'Виконане' AND order_date >= @from AND order_date < DATEADD(DAY, 1, @to) GROUP BY FORMAT(order_date, 'yyyy-MM') ORDER BY month_label",
                new SqlParameter("@from", dtpFrom.Value.Date), new SqlParameter("@to", dtpTo.Value.Date)).Rows)
            {
                chartMonth.Series[0].Points.AddXY(r["month_label"], r["total"]);
            }

            // Діаграма 2: товари
            chartProducts.Series[0].Points.Clear();
            foreach (DataRow r in Db.Query("SELECT p.product_name, SUM(oi.quantity * oi.unit_price) AS total FROM order_items oi JOIN orders o ON o.order_id = oi.order_id JOIN products p ON p.product_id = oi.product_id WHERE o.status = N'Виконане' AND o.order_date >= @from AND o.order_date < DATEADD(DAY, 1, @to) GROUP BY p.product_name ORDER BY total DESC",
                new SqlParameter("@from", dtpFrom.Value.Date), new SqlParameter("@to", dtpTo.Value.Date)).Rows)
            {
                chartProducts.Series[0].Points.AddXY(r["product_name"], r["total"]);
            }
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var fontTitle = new Font("Arial", 12, FontStyle.Bold);
            var fontHead = new Font("Consolas", 10, FontStyle.Bold);
            var font = new Font("Consolas", 10);
            float y = 40;

            e.Graphics.DrawString("Комп'ютерна фірма «Тех-Сервіс»", fontTitle, Brushes.Black, 60, y);
            y += 28;
            e.Graphics.DrawString("Звіт продажів з " + dtpFrom.Value.ToString("dd.MM.yyyy") + " по " + dtpTo.Value.ToString("dd.MM.yyyy"), new Font("Arial", 10), Brushes.Black, 60, y);
            y += 30;
            e.Graphics.DrawString(string.Format("{0,-6}{1,-12}{2,-35}{3,12}", "№", "Дата", "Клієнт", "Сума, грн"), fontHead, Brushes.Black, 60, y);
            y += 22;

            foreach (DataRowView r in _table.DefaultView)
            {
                e.Graphics.DrawString(string.Format("{0,-6}{1,-12}{2,-35}{3,12:0.00}", r["№"], Convert.ToDateTime(r["Дата"]).ToString("dd.MM.yyyy"), r["Клієнт"], Convert.ToDecimal(r["Сума"])), font, Brushes.Black, 60, y);
                y += 22;
            }
        }
    }
}