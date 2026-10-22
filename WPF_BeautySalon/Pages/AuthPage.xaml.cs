using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
// Не забудьте добавить ссылку на вашу модель БД, если она в другом пространстве имен
// using WPF_BeautySalon.Model; 

namespace WPF_BeautySalon.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        // Метод хэширования пароля
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // Обработка кнопки "Вход"
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            // 1. Проверка на пустые поля
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка капчи (если она активна)
            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                if (CaptchaInput.Text != CaptchaTextBox.Text)
                {
                    MessageBox.Show("Неверно введена капча!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    CaptchaChange();
                    CaptchaInput.Clear();
                    return;
                }
            }

            // 3. Хэширование введенного пароля
            string hashedPassword = GetHash(PasswordBox.Password);

            // 4. Подключение к БД и поиск пользователя
            try
            {
                // УБЕДИТЕСЬ, что у вас создана модель БД и класс называется DB_BeautySalonEntities (или переименуйте)
                using (var db = new DB_BeautySalonEntities())
                {
                    var user = db.User.AsNoTracking()
                        .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден или неверный пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        failedAttempts++;

                        // Если 3 раза ошиблись - включаем капчу
                        if (failedAttempts >= 3 && CaptchaPanel.Visibility != Visibility.Visible)
                        {
                            CaptchaPanel.Visibility = Visibility.Visible;
                            CaptchaChange();
                            MessageBox.Show("Слишком много попыток. Введите капчу.", "Внимание");
                        }
                        return;
                    }

                    // 5. Успешный вход
                    MessageBox.Show($"Добро пожаловать, {user.FIO}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очистка счетчика
                    failedAttempts = 0;

                    // Перенаправление по ролям
                    if (user.Role == "Admin" || user.Role == "Администратор")
                    {
                        NavigationService?.Navigate(new AdminPage());
                    }
                    else
                    {
                        NavigationService?.Navigate(new UserPage()); // Создайте эту страницу позже
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод генерации капчи
        private void CaptchaChange()
        {
            string allowchar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random r = new Random();
            string pwd = "";
            for (int i = 0; i < 5; i++) pwd += allowchar[r.Next(0, allowchar.Length)];
            CaptchaTextBox.Text = pwd;
        }

        // Обработка кнопки обновления капчи
        private void ButtonRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            CaptchaChange();
            CaptchaInput.Clear();
        }

        // Переход на страницу регистрации
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }
    }
}