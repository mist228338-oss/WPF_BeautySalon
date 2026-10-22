using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WPF_BeautySalon.Pages
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            DataGridCategory.ItemsSource = new DB_BeautySalonEntities().Category.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                var db = new DB_BeautySalonEntities();
                db.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                DataGridCategory.ItemsSource = db.Category.ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = (sender as Button)?.DataContext as Category;
            NavigationService?.Navigate(new AddCategoryPage(selectedCategory));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoriesForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();
            if (categoriesForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления!");
                return;
            }

            if (MessageBox.Show($"Удалить {categoriesForRemoving.Count} записей? Связанные платежи тоже удалятся (каскадно)!",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_BeautySalonEntities())
                    {
                        foreach (var category in categoriesForRemoving)
                        {
                            db.Category.Remove(category);
                        }
                        db.SaveChanges();
                        DataGridCategory.ItemsSource = db.Category.ToList();
                    }
                    MessageBox.Show("Данные удалены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}