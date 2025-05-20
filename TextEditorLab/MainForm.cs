using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TextEditorLab
{
    public partial class MainForm : Form
    {
        private Editor _editor;
        private RecentList _recentList;
        private int _newDocumentCounter = 1; // Для "Без имени 1", "Без имени 2" ...

        // Для хранения кнопок закрытия на вкладках
        private Dictionary<TabPage, Rectangle> _closeButtonRects = new Dictionary<TabPage, Rectangle>();

        // Ссылка на TabControl для Editor
        public TabControl tabControl => tabControlMain;


        public MainForm()
        {
            InitializeComponent();
            _editor = new Editor(this);
            _recentList = new RecentList();
            InitializeRecentFilesMenu();
            UpdateRecentFilesMenuItems(); // Первоначальное заполнение меню недавних файлов

            // Если нет вкладок, некоторые пункты меню должны быть неактивны
            UpdateMenuState();
        }

        // Метод для Editor
        public List<string> GetRecentFilesList() => _recentList.GetFilePaths();

        // Метод для Editor для обновления списка недавних файлов
        public void UpdateRecentFiles(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                _recentList.Add(filePath);
                UpdateRecentFilesMenuItems();
            }
        }


        // Инициализация меню для недавних файлов (чтобы не создавать каждый раз)
        private void InitializeRecentFilesMenu()
        {
            // Очищаем предыдущие элементы, если они были
            recentFilesToolStripMenuItem.DropDownItems.Clear();
            // Добавляем плейсхолдер или сообщение, если список пуст
            if (!_recentList.GetFilePaths().Any())
            {
                var noRecentItem = new ToolStripMenuItem("(пусто)");
                noRecentItem.Enabled = false;
                recentFilesToolStripMenuItem.DropDownItems.Add(noRecentItem);
            }
        }

        // Обновление подменю "Недавние файлы"
        private void UpdateRecentFilesMenuItems()
        {
            recentFilesToolStripMenuItem.DropDownItems.Clear();
            var paths = _recentList.GetFilePaths();

            if (paths.Count == 0)
            {
                var emptyItem = new ToolStripMenuItem("(пусто)");
                emptyItem.Enabled = false;
                recentFilesToolStripMenuItem.DropDownItems.Add(emptyItem);
            }
            else
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    var path = paths[i];
                    var item = new ToolStripMenuItem($"{i + 1}. {Path.GetFileName(path)}");
                    item.ToolTipText = path; // Показываем полный путь в подсказке
                    item.Tag = i; // Сохраняем индекс для OpenDocByRecentIndex
                    item.Click += RecentFile_Click;
                    recentFilesToolStripMenuItem.DropDownItems.Add(item);
                }
            }
        }

        private void RecentFile_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is int index)
            {
                _editor.OpenDocByRecentIndex(index);
            }
        }

        // Создание новой вкладки (вызывается из Editor)
        public void CreateNewTab(string filePath = null)
        {
            TabPage newTabPage = new TabPage();
            RichTextBox newRichTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10f) // Пример шрифта
            };
            newTabPage.Controls.Add(newRichTextBox);

            Document doc;
            if (string.IsNullOrEmpty(filePath))
            {
                doc = new Document(newRichTextBox);
                newTabPage.Text = $"Без имени {_newDocumentCounter++}"; // Начальное имя
                doc.IsModified = false; // Новый документ не изменен
            }
            else
            {
                doc = new Document(filePath, newRichTextBox); // Document.Open вызовется в конструкторе
                if (!doc.HasName) // Если открытие не удалось
                {
                    // Можно решить, что делать: закрыть вкладку или оставить как "Без имени"
                    // Пока просто оставим как есть, но с ошибкой при открытии
                    newTabPage.Text = $"Без имени {_newDocumentCounter++}";
                    doc.IsModified = false;
                }
                else
                {
                    newTabPage.Text = doc.StringShortName;
                }
            }

            newTabPage.Tag = doc; // Сохраняем документ в Tag вкладки
            tabControlMain.TabPages.Add(newTabPage);
            tabControlMain.SelectedTab = newTabPage;

            newRichTextBox.TextChanged += RichTextBox_TextChanged;
            UpdateMenuState();
            newTabPage.Text = doc.StringShortName; // Обновить заголовок после установки IsModified
        }

        // Открытие файла (вызывается из Editor)
        public void OpenFile(string filePath)
        {
            if (_editor.DocOpened(filePath)) // Проверяем, не открыт ли уже файл
            {
                return; // Если открыт, DocOpened его активирует
            }

            CreateNewTab(filePath); // CreateNewTab обработает загрузку

            // Обновляем список недавних файлов, если файл успешно открыт
            // CreateNewTab создает Document, который при открытии устанавливает FilePath
            // Если FilePath установлен, значит открытие прошло успешно
            var currentTab = tabControlMain.SelectedTab;
            if (currentTab != null && currentTab.Tag is Document currentDoc && currentDoc.HasName)
            {
                UpdateRecentFiles(currentDoc.FilePath);
            }
        }


        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab != null && tabControlMain.SelectedTab.Tag is Document doc)
            {
                if (!doc.IsModified) // Устанавливаем только если ранее не было установлено
                {
                    doc.IsModified = true;
                    tabControlMain.SelectedTab.Text = doc.StringShortName; // Обновить заголовок с "*"
                }
            }
        }

        // Обработчики меню
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.NewDoc();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.OpenDoc();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.SaveDoc();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.SaveDocAs();
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.CloseActiveDoc();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close(); // Вызовет FormClosing
        }

        // Логика закрытия вкладки
        public bool CloseTab(TabPage tabPage)
        {
            if (tabPage == null) return true; // Нечего закрывать

            if (tabPage.Tag is Document doc && doc.IsModified)
            {
                // Активируем вкладку перед показом диалога
                tabControlMain.SelectedTab = tabPage;

                var result = MessageBox.Show(
                    $"Сохранить изменения в файле \"{doc.StringShortName.TrimEnd('*')}\"?",
                    "Сохранение изменений",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (!doc.Save()) // Если Save вернул false (например, пользователь отменил SaveAs)
                    {
                        return false; // Не закрывать вкладку
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return false; // Не закрывать вкладку
                }
                // Если No, то просто продолжаем закрытие
            }

            tabControlMain.TabPages.Remove(tabPage);
            _closeButtonRects.Remove(tabPage); // Удаляем кнопку из словаря
            tabPage.Dispose(); // Освобождаем ресурсы вкладки
            UpdateMenuState();
            return true;
        }

        // Обновление состояния пунктов меню (активны/неактивны)
        private void UpdateMenuState()
        {
            bool hasTabs = tabControlMain.TabPages.Count > 0;
            saveToolStripMenuItem.Enabled = hasTabs;
            saveAsToolStripMenuItem.Enabled = hasTabs;
            closeTabToolStripMenuItem.Enabled = hasTabs;
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Можно обновить заголовок окна или другую информацию
            if (tabControlMain.SelectedTab != null && tabControlMain.SelectedTab.Tag is Document doc)
            {
                this.Text = $"Текстовый редактор - {doc.StringShortName}";
            }
            else
            {
                this.Text = "Текстовый редактор";
            }
        }

        // Событие перед закрытием формы
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Копируем коллекцию вкладок, так как она может изменяться во время итерации
            List<TabPage> tabsToProcess = new List<TabPage>(tabControlMain.TabPages.Cast<TabPage>());

            foreach (TabPage tab in tabsToProcess)
            {
                if (!CloseTab(tab)) // Если закрытие вкладки было отменено
                {
                    e.Cancel = true; // Отменить закрытие формы
                    return;
                }
            }
            // Сохраняем список недавних файлов перед выходом
            _recentList.SaveData();
        }

        // --- Логика для кнопок закрытия на вкладках ---
        private void tabControlMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Этот код рисует стандартную вкладку и кнопку "X"
            TabPage page = tabControlMain.TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);

            // Рисуем текст вкладки
            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, page.ForeColor);

            // Рисуем кнопку "X"
            // Размер кнопки
            int buttonSize = 12; // Немного меньше для лучшего вида
            int buttonMargin = 5; // Отступ кнопки от правого края вкладки

            Rectangle buttonRect = new Rectangle(
                e.Bounds.Right - buttonSize - buttonMargin,
                e.Bounds.Top + (e.Bounds.Height - buttonSize) / 2,
                buttonSize,
                buttonSize);

            // Сохраняем положение кнопки для обработки кликов
            _closeButtonRects[page] = buttonRect;


            // Рисуем крестик
            // Цвет крестика в зависимости от наведения (если бы мы отслеживали MouseMove на TabControl)
            // Для простоты, всегда черный
            using (Pen pen = new Pen(Color.DarkGray, 1.5f)) // Сделаем крестик потолще
            {
                // Немного уменьшим область крестика для красоты
                int crossPadding = 3;
                e.Graphics.DrawLine(pen, buttonRect.Left + crossPadding, buttonRect.Top + crossPadding,
                                         buttonRect.Right - crossPadding, buttonRect.Bottom - crossPadding);
                e.Graphics.DrawLine(pen, buttonRect.Right - crossPadding, buttonRect.Top + crossPadding,
                                         buttonRect.Left + crossPadding, buttonRect.Bottom - crossPadding);
            }

            // Рамка вокруг вкладки (опционально, для лучшего вида)
            // e.Graphics.DrawRectangle(Pens.LightGray, e.Bounds); 
        }

        private void tabControlMain_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControlMain.TabPages.Count; i++)
            {
                TabPage page = tabControlMain.TabPages[i];
                if (_closeButtonRects.ContainsKey(page))
                {
                    Rectangle buttonRect = _closeButtonRects[page];
                    if (buttonRect.Contains(e.Location))
                    {
                        CloseTab(page);
                        // Важно! Прервать дальнейшую обработку, чтобы не было переключения вкладок
                        // или других нежелательных действий после закрытия.
                        return;
                    }
                }
            }

            // Если клик был не по кнопке закрытия, но по заголовку, выбрать вкладку
            // Стандартное поведение TabControl может не срабатывать корректно с OwnerDrawFixed
            // без этой части, если клик происходит рядом с текстом, но не на нем.
            // Однако, обычно это не требуется, TabControl сам обрабатывает выбор.
            // Я оставлю это закомментированным, т.к. часто это избыточно.
            /* 
            for (int i = 0; i < tabControlMain.TabPages.Count; i++)
            {
                if (tabControlMain.GetTabRect(i).Contains(e.Location))
                {
                    tabControlMain.SelectedIndex = i;
                    break;
                }
            }
            */
        }
    }
}
