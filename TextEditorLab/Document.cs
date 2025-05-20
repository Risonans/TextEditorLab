using System.IO;
using System.Windows.Forms; // Для RichTextBox

namespace TextEditorLab
{
    public class Document
    {
        // Путь к файлу
        public string FilePath { get; private set; }

        // Содержимое документа (связано с RichTextBox)
        private RichTextBox _richTextBox;

        // Флаг, указывающий на наличие несохранённых изменений
        public bool IsModified { get; set; }

        // Конструктор для нового документа
        public Document(RichTextBox rtb)
        {
            _richTextBox = rtb;
            FilePath = null;
            IsModified = false; // Новый документ не изменен изначально (если пустой)
        }

        // Конструктор для существующего документа
        public Document(string filePath, RichTextBox rtb)
        {
            _richTextBox = rtb;
            Open(filePath); // Загружаем контент при открытии
        }

        // Проверка, задано ли имя файла
        public bool HasName => !string.IsNullOrEmpty(FilePath);

        // Полное имя файла (с путем)
        public string StringName => FilePath;

        // Короткое имя файла для отображения на вкладке
        public string StringShortName
        {
            get
            {
                string name = HasName ? Path.GetFileName(FilePath) : "Без имени";
                if (IsModified)
                {
                    name += "*";
                }
                return name;
            }
        }

        // Текст документа из RichTextBox
        public string Text
        {
            get => _richTextBox.Text;
            set => _richTextBox.Text = value;
        }

        // Открытие файла с указанным именем
        public bool Open(string filePath)
        {
            try
            {
                Text = File.ReadAllText(filePath);
                FilePath = filePath;
                IsModified = false;
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка открытия файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Сохранение текущего документа
        public bool Save()
        {
            if (!HasName)
            {
                // Если имя не задано, вызываем SaveAs
                return SaveAs();
            }

            try
            {
                File.WriteAllText(FilePath, Text);
                IsModified = false;
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Сохранение документа с новым именем (или впервые)
        public bool SaveAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                sfd.DefaultExt = "txt";
                sfd.FileName = HasName ? Path.GetFileName(FilePath) : "Без имени.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FilePath = sfd.FileName;
                    return Save(); // Вызываем обычное сохранение с новым путем
                }
            }
            return false; // Пользователь отменил сохранение
        }

        // Сохранение документа с указанным именем (используется в Editor.SaveDocAs)
        public bool SaveAs(string newFilePath)
        {
            FilePath = newFilePath;
            return Save();
        }
    }
}