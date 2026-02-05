using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using ExplorerSimple.Model;

namespace ExplorerSimple.ModelView
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        //ObservableCollection автоматически обновляет т.к. посылает событие CollectionChanged;
        public ObservableCollection<DirectoryItem> RootDirectories { get; } = new ObservableCollection<DirectoryItem>(); // 3 в потоке; TreeView отображает RootDirectories.

        private DirectoryItem _selectedDirectory;
        public DirectoryItem SelectedDirectory //5 в потоке. Сеттер SelectedDirectory вызывает LoadFiles(), заполняя SelectedDirectory.Files.
        {
            get => _selectedDirectory;
            set
            {
                if (_selectedDirectory == value) return;
                _selectedDirectory = value;
                // при выборе папки/диска загружаем файлы в модель
                _selectedDirectory?.LoadFiles();

                // Обновляем текущий путь
                CurrentPath = _selectedDirectory?.FullPath ?? "";

                OnPropertyChanged(nameof(SelectedDirectory)); //  6 в потоке. ListView отображает содержимое SelectedDirectory.Files.
            }
        }
        private string _currentPath;
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    OnPropertyChanged(nameof(CurrentPath));
                    OnPropertyChanged(nameof(CurrentPathWithLabel));
                }
            }
        }

        // Свойство для отображения с меткой
        public string CurrentPathWithLabel => $"Активный путь: {CurrentPath ?? ""}";
        public MainWindowViewModel()
        {
            LoadDrives();//2 в потоке; ViewModel в конструкторе помещает корневые диски в RootDirectories.
        }

        private void LoadDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                                      .Where(d => d.IsReady)
                                      .Select(d => new DirectoryItem
                                      {
                                          Name = $"💽 {d.Name} ({d.VolumeLabel})",
                                          FullPath = d.RootDirectory.FullName
                                      });

                foreach (var d in drives)
                {
                    RootDirectories.Add(d);
                }
            }
            catch
            {
                // игнорируем ошибки при получении списка дисков
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
