using System.Windows;
using System.Windows.Controls;

namespace WPF_BeautySalon.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new UsersTabPage());
        private void BtnCategories_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new CategoryTabPage());
        private void BtnPayments_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new PaymentTabPage());
        private void BtnCharts_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new DiagramPage());
    }
}