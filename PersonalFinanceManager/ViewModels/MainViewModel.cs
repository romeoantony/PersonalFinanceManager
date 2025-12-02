using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PersonalFinanceManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;



        [ObservableProperty]
        private ObservableCollection<Expense> expenses;

        [ObservableProperty]
        private ObservableCollection<Category> categories;

        [ObservableProperty]
        private decimal totalExpenses;

        [ObservableProperty]
        private decimal currentMonthTotal;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Now;

        [ObservableProperty]
        private string newExpenseDescription;

        [ObservableProperty]
        private decimal newExpenseAmount;

        [ObservableProperty]
        private Category selectedCategory;

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            Expenses = new ObservableCollection<Expense>();
            Categories = new ObservableCollection<Category>();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            var cats = await _databaseService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in cats) Categories.Add(cat);
            if (Categories.Any()) SelectedCategory = Categories.First();

            await LoadExpensesAsync();
        }

        private async Task LoadExpensesAsync()
        {
            var exps = await _databaseService.GetExpensesAsync();
            Expenses.Clear();
            foreach (var exp in exps) Expenses.Add(exp);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            TotalExpenses = Expenses.Sum(e => e.Amount);
            CurrentMonthTotal = Expenses.Where(e => e.Date.Month == DateTime.Now.Month && e.Date.Year == DateTime.Now.Year).Sum(e => e.Amount);
            // UpdateChart();
        }



        [RelayCommand]
        private async Task AddExpense()
        {
            if (SelectedCategory == null || NewExpenseAmount <= 0) return;

            var expense = new Expense
            {
                Date = SelectedDate,
                Amount = NewExpenseAmount,
                Description = NewExpenseDescription,
                CategoryId = SelectedCategory.Id
            };

            await _databaseService.AddExpenseAsync(expense);
            
            // Reset UI
            NewExpenseAmount = 0;
            NewExpenseDescription = string.Empty;
            
            await LoadExpensesAsync();
        }

        [RelayCommand]
        private void ExportToExcel()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Expenses");

            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "Category";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Amount";

            int row = 2;
            foreach (var expense in Expenses)
            {
                worksheet.Cell(row, 1).Value = expense.Date;
                worksheet.Cell(row, 2).Value = expense.Category.Name;
                worksheet.Cell(row, 3).Value = expense.Description;
                worksheet.Cell(row, 4).Value = expense.Amount;
                row++;
            }

            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Expenses.xlsx");
            workbook.SaveAs(path);
            System.Windows.MessageBox.Show($"Exported to {path}");
        }

        [RelayCommand]
        private void ExportToPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Expenses.pdf");

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("Expense Report").FontSize(20).SemiBold().AlignCenter();
                    
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Category").Bold();
                            header.Cell().Text("Description").Bold();
                            header.Cell().Text("Amount").Bold();
                        });

                        foreach (var expense in Expenses)
                        {
                            table.Cell().Text(expense.Date.ToShortDateString());
                            table.Cell().Text(expense.Category.Name);
                            table.Cell().Text(expense.Description);
                            table.Cell().Text(expense.Amount.ToString("C"));
                        }
                    });
                });
            }).GeneratePdf(path);

            System.Windows.MessageBox.Show($"Exported to {path}");
        }
    }
}
