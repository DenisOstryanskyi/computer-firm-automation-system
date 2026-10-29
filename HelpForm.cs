using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CFAS
{
    /// <summary>Довідка: інструкція користувача за розділами та контекстна допомога (F1).</summary>
    public class HelpForm : Form
    {
        public const string TopicLogin = "Вхід у систему";
        public const string TopicMain = "Головне вікно та ролі";
        public const string TopicDirectories = "Довідники";
        public const string TopicOrders = "Замовлення";
        public const string TopicSearch = "Пошук, сортування, фільтрація";
        public const string TopicReports = "Звіти і друк";
        public const string TopicJournal = "Журнал подій";
        public const string TopicUsers = "Користувачі та налаштування";
        public const string TopicTrouble = "Усунення несправностей";
        public const string TopicKeys = "Гарячі клавіші";

        private readonly ListBox lstTopics = new ListBox();
        private readonly TextBox txtHelp = new TextBox();
        private readonly List<KeyValuePair<string, string>> _topics = new List<KeyValuePair<string, string>>();

        public HelpForm() : this(null) { }

        public HelpForm(string topicTitle)
        {
            Text = "Довідка CFAS";
            Icon = Icons.WindowIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 460);

            FillTopics();

            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            lstTopics.Dock = DockStyle.Left;
            lstTopics.Width = 210;
            lstTopics.SelectedIndexChanged += (s, e) => {
                if (lstTopics.SelectedIndex >= 0)
                    txtHelp.Text = _topics[lstTopics.SelectedIndex].Value;
            };

            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.ScrollBars = ScrollBars.Vertical;
            txtHelp.Dock = DockStyle.Fill;
            txtHelp.BackColor = SystemColors.Window;
            txtHelp.Font = new Font("Segoe UI", 10F);

            foreach (var topic in _topics) lstTopics.Items.Add(topic.Key);

            Controls.Add(txtHelp);
            Controls.Add(lstTopics);

            int index = topicTitle == null ? 0 : lstTopics.FindStringExact(topicTitle);
            lstTopics.SelectedIndex = index >= 0 ? index : 0;
        }

        /// <summary>Відкриває довідку на потрібному розділі.</summary>
        public static void ShowTopic(IWin32Window owner, string topicTitle)
        {
            using (var form = new HelpForm(topicTitle))
            {
                form.ShowDialog(owner);
            }
        }

        /// <summary>Підключає до форми контекстну допомогу: клавіша F1 відкриває вказаний розділ.</summary>
        public static void AttachF1(Form form, string topicTitle)
        {
            form.KeyPreview = true;
            form.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.F1)
                {
                    e.Handled = true;
                    ShowTopic(form, topicTitle);
                }
            };
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private void Add(string title, string text)
        {
            _topics.Add(new KeyValuePair<string, string>(title, text));
        }

        private void FillTopics()
        {
            Add(TopicLogin, Lines(
                "ВХІД У СИСТЕМУ",
                "",
                "1. Запустіть CFAS. Відкриється вікно «Вхід у систему CFAS».",
                "2. Введіть логін і пароль та натисніть «Увійти» (або Enter).",
                "3. Після успішного входу відкриється головне вікно; у його заголовку показано ваше ПІБ і роль.",
                "",
                "Якщо пароль невірний або обліковий запис вимкнено, з’явиться повідомлення «Невірний логін або пароль». Кожну спробу входу, вдалу чи невдалу, записано в журнал подій.",
                "Для виходу закрийте головне вікно: подію виходу також буде записано в журнал."));

            Add(TopicMain, Lines(
                "ГОЛОВНЕ ВІКНО ТА РОЛІ",
                "",
                "Меню «Довідники»: клієнти і товари.",
                "Меню «Операції»: замовлення.",
                "Меню «Звіти»: звіт продажів.",
                "Меню «Сервіс»: журнал подій, користувачі, налаштування (лише для адміністратора).",
                "Меню «Довідка»: ця довідка та вікно «Про програму».",
                "",
                "Адміністратор має повний доступ. Менеджер і працівник працюють із довідниками, замовленнями та звітами; меню «Сервіс» для них приховано."));

            Add(TopicDirectories, Lines(
                "ДОВІДНИКИ (КЛІЄНТИ ТА ТОВАРИ)",
                "",
                "Додати: натисніть «Додати», заповніть поля й натисніть «OK». Обов’язкові поля: ПІБ клієнта; назва, ціна й залишок товару.",
                "Редагувати: виберіть рядок і натисніть «Редагувати».",
                "Видалити: виберіть рядок, натисніть «Видалити» й підтвердіть дію.",
                "",
                "Клієнта, у якого є замовлення, і товар, який використовується в замовленнях, видалити не можна: програма покаже відповідне повідомлення.",
                "Ціна і залишок не можуть бути від’ємними."));

            Add(TopicOrders, Lines(
                "ЗАМОВЛЕННЯ",
                "",
                "1. Меню «Операції» → «Замовлення» → «Створити».",
                "2. Виберіть клієнта, статус (Нове, Виконується, Виконане, Скасоване), за потреби введіть коментар.",
                "3. Виберіть товар, вкажіть кількість і натисніть «Додати». Ціна підставляється автоматично, поруч показано залишок на складі.",
                "4. Помилково додану позицію можна видалити кнопкою «Прибрати».",
                "5. Натисніть «Зберегти»: товари автоматично спишуться зі складу.",
                "",
                "Не можна додати більше товару, ніж є на складі. Повторне збереження відкритого замовлення не списує товар вдруге. При видаленні замовлення товар повертається на склад."));

            Add(TopicSearch, Lines(
                "ПОШУК, СОРТУВАННЯ, ФІЛЬТРАЦІЯ",
                "",
                "Пошук: введіть текст у поле над таблицею; таблиця відфільтрується під час введення (клієнти: ПІБ і телефон; товари: назва і категорія; замовлення: клієнт і статус; користувачі: логін і ПІБ).",
                "Сортування: клацніть заголовок колонки; повторне клацання змінює напрям, стрілка показує поточне сортування.",
                "Фільтрація: у журналі подій — за типом події та датою «від»; у звіті — за періодом."));

            Add(TopicReports, Lines(
                "ЗВІТИ І ДРУК",
                "",
                "1. Меню «Звіти» → «Звіт продажів».",
                "2. Вкажіть період («з» і «по») і натисніть «Побудувати».",
                "3. Програма покаже таблицю виконаних замовлень, їх кількість і загальну суму, діаграму продажів за місяцями та за товарами.",
                "4. Кнопка «Друк» відкриває попередній перегляд; звідти звіт можна надрукувати.",
                "",
                "До звіту потрапляють лише замовлення зі статусом «Виконане»."));

            Add(TopicJournal, Lines(
                "ЖУРНАЛ ПОДІЙ (АДМІНІСТРАТОР)",
                "",
                "Меню «Сервіс» → «Журнал подій». У таблиці показано час, користувача, тип і опис події.",
                "Типи подій: LOGIN (вхід), LOGIN_FAIL (невдалий вхід), LOGOUT (вихід), INSERT, UPDATE, DELETE (зміни даних), ORDER (операції із замовленнями), SYSTEM (зміна налаштувань).",
                "Виберіть тип і дату «від», щоб відфільтрувати записи; кнопка «Оновити» перечитує журнал."));

            Add(TopicUsers, Lines(
                "КОРИСТУВАЧІ ТА НАЛАШТУВАННЯ (АДМІНІСТРАТОР)",
                "",
                "Користувачі: «Сервіс» → «Користувачі». Додавання: логін, пароль, ПІБ, роль. Редагування: якщо поле пароля залишити порожнім, пароль не зміниться. Прапорець «Активний» дозволяє або забороняє вхід.",
                "Налаштування: «Сервіс» → «Налаштування». Виберіть параметр у таблиці, введіть нове значення й натисніть «Застосувати». Зміну буде записано в журнал."));

            Add(TopicTrouble, Lines(
                "УСУНЕННЯ НЕСПРАВНОСТЕЙ",
                "",
                "«Не вдалося підключитися до бази даних»: перевірте, що встановлено SQL Server LocalDB (команда sqllocaldb info у командному рядку), і що в файлі CFAS.exe.config правильний рядок підключення.",
                "«Невірний логін або пароль»: перевірте розкладку клавіатури та Caps Lock; зверніться до адміністратора, якщо обліковий запис вимкнено.",
                "«Недостатньо товару на складі»: зменште кількість або поповніть залишок у довіднику товарів.",
                "Інші проблеми: контакти технічної підтримки наведено у вікні «Довідка» → «Про програму»."));

            Add(TopicKeys, Lines(
                "ГАРЯЧІ КЛАВІШІ",
                "",
                "F1 — довідка за поточним розділом.",
                "Enter — підтвердити (кнопка «Увійти», «OK», «Зберегти»).",
                "Esc — закрити діалог без збереження."));
        }
    }
}
