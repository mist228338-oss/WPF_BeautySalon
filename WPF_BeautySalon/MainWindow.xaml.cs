using System;
using System.Windows;
using System.Windows.Controls;

namespace WPF_BeautySalon
{
    public partial class MainWindow : Window
    {
        // Флаг для отслеживания текущей темы (по умолчанию светлая - false)
        private bool isDarkTheme = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        // 1. Загрузка окна: запуск часов в правом нижнем углу
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                // Формат: 24.10.2026 15:30:00
                DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            };
            timer.Start();
        }

        // 2. Закрытие окна (крестик): запрос подтверждения
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }

        // 3. Кнопка "Назад": возвращает на предыдущую страницу
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        // 4. Кнопка "Сменить тему": переключает светлую и темную темы
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Выбираем файл: если сейчас темная (true), грузим светлую, и наоборот
                string themeFile = isDarkTheme ? "Dictionary.xaml" : "DarkDictionary.xaml";

                // Создаем путь к файлу
                var uri = new Uri(themeFile, UriKind.Relative);

                // Загружаем словарь ресурсов
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

                if (resourceDict != null)
                {
                    // Очищаем старые стили и применяем новые
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);

                    // Меняем флаг на противоположный
                    isDarkTheme = !isDarkTheme;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки темы: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}