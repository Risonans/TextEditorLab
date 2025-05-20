using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TextEditorLab
{
    // Этот класс будет содержать основную логику, как указано в ТЗ.
    // MainForm будет вызывать его методы.
    public class Editor
    {
        private MainForm _mainForm; // Ссылка на главную форму для доступа к TabControl и др.

        public Editor(MainForm mainForm)
        {
            _mainForm = mainForm;
        }

        // Создание нового документа (вкладки)
        public void NewDoc()
        {
            _mainForm.CreateNewTab(); // MainForm содержит логику создания UI
        }

        // Закрытие активного документа с проверкой на сохранение
        public void CloseActiveDoc()
        {
            if (_mainForm.tabControl.TabPages.Count == 0) return;

            TabPage activeTab = _mainForm.tabControl.SelectedTab;
            if (activeTab != null)
            {
                _mainForm.CloseTab(activeTab); // MainForm управляет закрытием вкладок
            }
        }

        // Открытие документа из файловой системы
        public void OpenDoc()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _mainForm.OpenFile(ofd.FileName); // MainForm управляет открытием
                }
            }
        }

        // Сохранение текущего активного документа
        public void SaveDoc()
        {
            if (_mainForm.tabControl.TabPages.Count == 0) return;

            TabPage activeTab = _mainForm.tabControl.SelectedTab;
            if (activeTab != null && activeTab.Tag is Document doc)
            {
                if (doc.Save()) // Метод Save в Document обрабатывает Save As, если нужно
                {
                    activeTab.Text = doc.StringShortName; // Обновить заголовок вкладки
                    _mainForm.UpdateRecentFiles(doc.FilePath); // Обновить список недавних
                }
            }
        }

        // Сохранение текущего активного документа с выбором нового пути и имени файла
        public void SaveDocAs()
        {
            if (_mainForm.tabControl.TabPages.Count == 0) return;

            TabPage activeTab = _mainForm.tabControl.SelectedTab;
            if (activeTab != null && activeTab.Tag is Document doc)
            {
                if (doc.SaveAs()) // Вызываем SaveAs из Document
                {
                    activeTab.Text = doc.StringShortName; // Обновить заголовок вкладки
                    _mainForm.UpdateRecentFiles(doc.FilePath); // Обновить список недавних
                }
            }
        }

        // Открытие документа из списка недавно открытых файлов по индексу
        public void OpenDocByRecentIndex(int index)
        {
            var recentFiles = _mainForm.GetRecentFilesList();
            if (index >= 0 && index < recentFiles.Count)
            {
                _mainForm.OpenFile(recentFiles[index]);
            }
        }

        // Проверка, открыт ли уже документ с заданным именем
        public bool DocOpened(string fileName)
        {
            foreach (TabPage tab in _mainForm.tabControl.TabPages)
            {
                if (tab.Tag is Document doc && doc.HasName)
                {
                    // Сравниваем полные пути без учета регистра
                    if (string.Equals(doc.FilePath, fileName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        _mainForm.tabControl.SelectedTab = tab; // Активируем вкладку
                        return true;
                    }
                }
            }
            return false;
        }
    }
}