using System.Collections.Generic;
using System.IO;
using System.Linq; // Для Distinct и Take

namespace TextEditorLab
{
    public class RecentList
    {
        private const int MaxRecentFiles = 5;
        private List<string> _filePaths;
        private readonly string _settingsFilePath; // Путь к файлу для сохранения списка

        public RecentList(string settingsFileName = "recent_files.txt")
        {
            // Сохраняем в папке данных приложения
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "TextEditorLab");
            Directory.CreateDirectory(appFolder); // Создаем папку, если ее нет
            _settingsFilePath = Path.Combine(appFolder, settingsFileName);

            _filePaths = new List<string>();
            LoadData();
        }

        // Добавление файла в список недавно открытых файлов
        public void Add(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            // Удаляем, если уже есть, чтобы переместить наверх
            _filePaths.RemoveAll(f => f.Equals(fileName, System.StringComparison.OrdinalIgnoreCase));

            _filePaths.Insert(0, fileName); // Добавляем в начало

            // Ограничиваем количество
            if (_filePaths.Count > MaxRecentFiles)
            {
                _filePaths = _filePaths.Take(MaxRecentFiles).ToList();
            }
            SaveData();
        }

        // Сохранение списка недавно открытых файлов
        public void SaveData()
        {
            try
            {
                File.WriteAllLines(_settingsFilePath, _filePaths);
            }
            catch (System.Exception ex)
            {
                // Можно добавить логирование или тихо проигнорировать
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения списка недавних файлов: {ex.Message}");
            }
        }

        // Загрузка списка недавно открытых файлов при запуске приложения
        public void LoadData()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    _filePaths.Clear();
                    _filePaths.AddRange(File.ReadAllLines(_settingsFilePath)
                                        .Where(f => !string.IsNullOrWhiteSpace(f) && File.Exists(f)) // Проверяем существование файла
                                        .Distinct(System.StringComparer.OrdinalIgnoreCase) // Убираем дубликаты
                                        .Take(MaxRecentFiles)); // Берем не больше MaxRecentFiles
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки списка недавних файлов: {ex.Message}");
                    _filePaths.Clear(); // В случае ошибки очищаем список
                }
            }
        }

        // Получить список файлов
        public List<string> GetFilePaths()
        {
            // Обновим список, отфильтровав несуществующие файлы перед возвратом
            _filePaths.RemoveAll(f => !File.Exists(f));
            return new List<string>(_filePaths); // Возвращаем копию
        }
    }
}