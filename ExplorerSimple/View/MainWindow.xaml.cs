using System.Windows;
using System.Windows.Controls;
using ExplorerSimple.Model;
using ExplorerSimple.ModelView;

namespace ExplorerSimple
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); //Приложение создаёт MainWindow и устанавливает DataContext = MainWindowViewModel.
            DataContext = new MainWindowViewModel(); //1 в потоке
        }

        // При изменении выбранного элемента TreeView передаём его VM
        private void RecursTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) // 4 в потоке. событие SelectedItemChanged вызывает vm.SelectedDirectory = выбранный DirectoryItem.
        {
            if (DataContext is MainWindowViewModel vm)
            {
                // Устанавливаем новый выбранный элемент
                if (e.NewValue is DirectoryItem newItem)
                {
                    newItem.IsSelected = true;
                    vm.SelectedDirectory = newItem;
                }
                else
                {
                    vm.SelectedDirectory = null;
                }
            }
        }
        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && !string.IsNullOrEmpty(vm.CurrentPath))
            {
                Clipboard.SetText(vm.CurrentPath);
            }
        }

        private void UpdateCurrentPathLAbel(string fullPath)
        {
            // Обновляем только путь, сохраняя префикс "Активный путь: "
            CurrentPathText.Text = $"Активный путь: {fullPath}";
        }
    }
}
