using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WPF_BeautySalon.Pages
{
    public partial class RegPage : Page
    {
        // Конструктор страницы (обязателен для работы XAML)
        public RegPage()
        {
            InitializeComponent();
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка на заполнение всех полей
            if (string.IsNullOrEmpty(txtbxLog.Text) || string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) || string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка формата пароля (минимум 6 символов, англ. буквы, хотя бы 1 цифра)
            if (passBxFrst.Password.Length < 6 || !passBxFrst.Password.Any(char.IsDigit) || passBxFrst.Password.Any(c => c > 127))
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов, цифру и только английские буквы!", "Ошибка");
                return;
            }

            // 3. Проверка на совпадение паролей
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка");
                return;
            }

            // 4. Сохранение в БД
            try
            {
                using (var db = new DB_BeautySalonEntities())
                {
                    // Проверка, нет ли уже такого логина
                    if (db.User.Any(u => u.Login == txtbxLog.Text))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка");
                        return;
                    }

                    // Создаем нового пользователя
                    var newUser = new User
                    {
                        Login = txtbxLog.Text,
                        Password = AuthPage.GetHash(passBxFrst.Password), // Берем метод хэширования из AuthPage
                        Role = comboBox.Text,
                        FIO = txtbxFIO.Text
                    };

                    db.User.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Возвращаемся на страницу авторизации
                    NavigationService?.Navigate(new AuthPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка");
            }
        }

        // Кнопка возврата на страницу входа (из XAML)
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }
    }
}