using System;
using System.Windows;
using System.Windows.Controls;

namespace WPF_BeautySalon.Pages
{
    public partial class AddCategoryPage : Page
    {
        private Category _currentCategory;

        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();

            if (selectedCategory != null)
            {
                _currentCategory = selectedCategory;
                TbTitle.Text = "Редактирование категории";
            }
            else
            {
                _currentCategory = new Category();
            }

            DataContext = _currentCategory;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentCategory.Name))
            {
                MessageBox.Show("Введите название категории!", "Ошибка");
                return;
            }

            try
            {
                if (_currentCategory.ID == 0)
                    new DB_BeautySalonEntities().Category.Add(_currentCategory);

                new DB_BeautySalonEntities().SaveChanges();
                MessageBox.Show("Успешно сохранено!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}