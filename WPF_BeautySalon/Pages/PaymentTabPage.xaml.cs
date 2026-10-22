using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity; // Для метода Include

namespace WPF_BeautySalon.Pages
{
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DataGridPayments.ItemsSource = new DB_BeautySalonEntities().Payment
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var paymentsForRemoving = DataGridPayments.SelectedItems.Cast<Payment>().ToList();

            if (paymentsForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления!");
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите удалить {paymentsForRemoving.Count} записей?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_BeautySalonEntities())
                    {
                        foreach (var payment in paymentsForRemoving)
                        {
                            db.Payment.Remove(payment);
                        }
                        db.SaveChanges();
                    }

                    MessageBox.Show("Данные успешно удалены!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }
    }
}