using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ExplorerSimple.Model
{
    public class DirectoryItem : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;

        private bool _isExpand;
        public bool IsExpanded
        {
            get
            {
                return _isExpand; 
            }
            set
            {
                if(_isExpand != value)
                {
                    _isExpand = value;
                    OnPropertyChanged(nameof(IsExpanded));

                    // Ленивая загрузка при раскрытии
                    if(value && !string.IsNullOrEmpty(FullPath))
                    {
                        if(Directories.Count == 0)
                        {
                            LoadSubdirectories();
                        }
                    }
                    else
                    {
                        Directories.Clear();// Очищаем подпапки при закрытии// Но лучше убрать, чтобы остались загруженными для быстрого доступа

                        // Рекурсивно закрываем все дочерние элементы
                        CloseAllChildDirectories(this);
                    }
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected; 
            }
            set
            {
                if(_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));

                    // Загрузка файлов при выборе элемента
                    if (value)
                    {
                        IsExpanded = true;// Автоматически раскрываем при выборе и загружаем файлы //если не сразу, то можно убрать эту строчку
                        LoadFiles();
                    }
                }
            }
        }

        // Коллекция файлов для отображения в ListView
        // Простая синхронная загрузка списка файлов (применима для дисков/корневых папок)
        public ObservableCollection<FileItem> Files { get; set; } = new ObservableCollection<FileItem>();
        public ObservableCollection<DirectoryItem> Directories { get; set; } = new ObservableCollection<DirectoryItem>();

        public void CloseAllChildDirectories(DirectoryItem item)
        {
            if (item != null) return;
            foreach (var child in item.Directories)
            {
                // Закрываем дочерний элемент
                child.IsExpanded = false;
                // Рекурсивно закрываем его детей
                CloseAllChildDirectories(child);
            }
        }

        public void LoadFiles()
        {
            Files.Clear();
            try
            {
                var files = Directory.GetFiles(FullPath)
                                     .Select(f => new FileItem { FileName = $"📄 Path.GetFileName(f)" });

                foreach (var f in files)
                {
                    Files.Add(f);
                }
            }
            catch
            {
                // Если нет доступа или другая ошибка — просто оставим коллекцию пустой.
            }
            
        }
        public void LoadSubdirectories()
        {
            try
            {
                var dirs = Directory.GetDirectories(FullPath)
                    .Select(f => new DirectoryItem { Name = $"📁 {Path.GetFileName(f)}", FullPath = f });
                foreach (var f in dirs)
                {
                    Directories.Add(f);
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
