using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    public class HelpForm : Form
    {
        private readonly ListBox lstTopics = new ListBox();
        private readonly TextBox txtHelp = new TextBox();

        private readonly Dictionary<string, string> _topics = new Dictionary<string, string>
        {
            { "Вхід у систему", "Введіть логін і пароль та натисніть «Увійти».\nПароль перевіряється через SHA-256 хеш." },
            { "Довідники", "Довідники містять клієнтів і товари.\nДоступні перегляд, пошук, додавання, редагування та видалення." },
            { "Замовлення", "Замовлення складається з клієнта, статусу та списку товарів.\nПри збереженні товари автоматично списуються зі складу." },
            { "Звіти", "Звіт продажів показує виконані замовлення за обраний період.\nДані відображаються у вигляді таблиці та графіків." },
            { "Журнал подій", "Журнал (доступний адміну) фіксує всі дії користувачів та входи в систему." },
            { "Ролі", "Адміністратор має повний доступ.\nМенеджер і працівник працюють із замовленнями та довідниками." }
        };

        public HelpForm()
        {
            Text = "Довідка CFAS";
            Icon = Icons.WindowIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(700, 420);

            lstTopics.Dock = DockStyle.Left;
            lstTopics.Width = 190;
            lstTopics.SelectedIndexChanged += (s, e) => {
                if (lstTopics.SelectedItem != null)
                    txtHelp.Text = _topics[lstTopics.SelectedItem.ToString()].Replace("\n", Environment.NewLine);
            };

            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.ScrollBars = ScrollBars.Vertical;
            txtHelp.Dock = DockStyle.Fill;

            foreach (string topic in _topics.Keys) lstTopics.Items.Add(topic);

            Controls.Add(txtHelp);
            Controls.Add(lstTopics);

            if (lstTopics.Items.Count > 0) lstTopics.SelectedIndex = 0;
        }
    }
}