using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WPF_BeautySalon.Pages
{
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            UpdateUsers();
        }

        private void UpdateUsers()
        {
            if (!IsInitialized) return;

            try
            {
                var currentUsers = new DB_BeautySalonEntities().User.ToList();

                if (!string.IsNullOrWhiteSpace(TxtFilterFio.Text))
                {
                    currentUsers = currentUsers.Where(x => x.FIO.ToLower().Contains(TxtFilterFio.Text.ToLower())).ToList();
                }

                DataGridUsers.ItemsSource = (CmbSort.SelectedIndex == 0)
                    ? currentUsers.OrderBy(x => x.FIO).ToList()
                    : currentUsers.OrderByDescending(x => x.FIO).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void TxtFilterFio_TextChanged(object sender, TextChangedEventArgs e) => UpdateUsers();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateUsers();

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            TxtFilterFio.Text = "";
            CmbSort.SelectedIndex = 0;
            UpdateUsers();
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DataGridUsers.SelectedItems.Cast<User>().ToList();

            if (usersForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления!");
                return;
            }

            if (MessageBox.Show($"Удалить пользователя {usersForRemoving[0].FIO}? Все его платежи тоже будут удалены!",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_BeautySalonEntities())
                    {
                        foreach (var user in usersForRemoving)
                        {
                            db.User.Remove(user);
                        }
                        db.SaveChanges();
                    }

                    MessageBox.Show("Пользователь успешно удален!");
                    UpdateUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }
    }
}