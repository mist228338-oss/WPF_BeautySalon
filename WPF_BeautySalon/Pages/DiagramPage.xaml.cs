using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace WPF_BeautySalon.Pages
{
    public partial class DiagramPage : Page
    {
        // Объявляем элементы графика
        private Chart myChart;
        private WindowsFormsHost winFormsHost;

        public DiagramPage()
        {
            InitializeComponent();

            // 1. Программно создаем Chart и помещаем его в ChartContainer
            SetupChart();

            // 2. Загружаем данные в ComboBox
            using (var db = new DB_BeautySalonEntities())
            {
                CmbUser.ItemsSource = db.User.ToList();
            }

            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

            // Устанавливаем значения по умолчанию, чтобы график построился сразу
            CmbUser.SelectedIndex = 0;
            CmbDiagram.SelectedIndex = 0;
        }

        // Метод создания графика
        private void SetupChart()
        {
            winFormsHost = new WindowsFormsHost();
            myChart = new Chart();

            // Настраиваем область графика
            ChartArea chartArea = new ChartArea("MainArea");
            myChart.ChartAreas.Add(chartArea);

            // Настраиваем легенду
            Legend legend = new Legend("Legend1");
            myChart.Legends.Add(legend);

            // Помещаем Chart в хост, а хост — в наш Grid из XAML
            winFormsHost.Child = myChart;
            ChartContainer.Children.Add(winFormsHost);
        }

        // Метод обновления графика при выборе пользователя/типа
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser && CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                myChart.Series.Clear();

                Series series = new Series("Сумма расходов");
                series.ChartType = currentType;
                series.IsValueShownAsLabel = true; // Показывать цифры

                // Получаем данные из БД
                using (var db = new DB_BeautySalonEntities())
                {
                    var payments = db.Payment
                        .Where(p => p.UserID == currentUser.ID)
                        .GroupBy(p => p.Category.Name)
                        .Select(g => new { Category = g.Key, Sum = g.Sum(p => p.Price * p.Num) })
                        .ToList();

                    if (payments.Count == 0)
                    {
                        myChart.Titles.Clear();
                        myChart.Titles.Add("У этого клиента пока нет платежей");
                    }
                    else
                    {
                        myChart.Titles.Clear();
                        foreach (var item in payments)
                        {
                            series.Points.AddXY(item.Category, item.Sum);
                        }
                    }
                }
                myChart.Series.Add(series);
            }
        }

        // ==========================================
        // ЭКСПОРТ В WORD
        // ==========================================
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DB_BeautySalonEntities())
                {
                    var allUsers = db.User.ToList().OrderBy(u => u.FIO).ToList();

                    var application = new Word.Application();
                    application.Visible = true;
                    var document = application.Documents.Add();

                    foreach (var user in allUsers)
                    {
                        // Заголовок с ФИО
                        Word.Paragraph titleParagraph = document.Paragraphs.Add();
                        Word.Range titleRange = titleParagraph.Range;
                        titleRange.Text = user.FIO;
                        titleParagraph.set_Style("Заголовок 1");
                        titleRange.InsertParagraphAfter();

                        // Таблица
                        var payments = user.Payment.ToList();
                        Word.Table paymentsTable = document.Tables.Add(document.Paragraphs.Add().Range, payments.Count + 1, 3);
                        paymentsTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                        // Шапка таблицы
                        paymentsTable.Cell(1, 1).Range.Text = "Дата";
                        paymentsTable.Cell(1, 2).Range.Text = "Услуга";
                        paymentsTable.Cell(1, 3).Range.Text = "Сумма";
                        paymentsTable.Rows[1].Range.Font.Bold = 1;

                        // Данные
                        for (int i = 0; i < payments.Count; i++)
                        {
                            var p = payments[i];
                            paymentsTable.Cell(i + 2, 1).Range.Text = p.Date.ToString("dd.MM.yyyy");
                            paymentsTable.Cell(i + 2, 2).Range.Text = p.Name;
                            paymentsTable.Cell(i + 2, 3).Range.Text = (p.Price * p.Num).ToString("N2") + " руб.";
                        }

                        document.Paragraphs.Add(); // Отступ между пользователями
                        if (user != allUsers.Last()) document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта в Word: " + ex.Message);
            }
        }

        // ==========================================
        // ЭКСПОРТ В EXCEL
        // ==========================================
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DB_BeautySalonEntities())
                {
                    var allUsers = db.User.ToList().OrderBy(u => u.FIO).ToList();
                    var application = new Excel.Application();
                    application.Visible = true;

                    Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                    // Оставляем только один лист
                    while (workbook.Sheets.Count > 1)
                    {
                        workbook.Sheets[workbook.Sheets.Count].Delete();
                    }

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var user = allUsers[i];
                        Excel.Worksheet worksheet = (i == 0) ? (Excel.Worksheet)workbook.Sheets[1] : (Excel.Worksheet)workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);

                        // Обрезаем имя листа, если оно длиннее 31 символа
                        worksheet.Name = user.FIO.Length > 31 ? user.FIO.Substring(0, 31) : user.FIO;

                        // Шапка
                        worksheet.Cells[1, 1] = "Категория";
                        worksheet.Cells[1, 2] = "Сумма расходов";
                        worksheet.Cells[1, 1].Font.Bold = true;
                        worksheet.Cells[1, 2].Font.Bold = true;

                        // Запрос данных
                        var categorySums = user.Payment
                            .GroupBy(p => p.Category.Name)
                            .Select(g => new { CatName = g.Key, Total = g.Sum(p => p.Price * p.Num) })
                            .ToList();

                        // Заполнение
                        for (int j = 0; j < categorySums.Count; j++)
                        {
                            worksheet.Cells[j + 2, 1] = categorySums[j].CatName;
                            worksheet.Cells[j + 2, 2] = categorySums[j].Total;
                        }

                        worksheet.Columns.AutoFit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта в Excel: " + ex.Message);
            }
        }
    }
}