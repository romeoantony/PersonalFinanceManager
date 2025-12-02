using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Data
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task AddExpenseAsync(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Expense>> GetExpensesAsync()
        {
            return await _context.Expenses.Include(e => e.Category).OrderByDescending(e => e.Date).ToListAsync();
        }
        
        public async Task<List<Expense>> GetExpensesByMonthAsync(int month, int year)
        {
             return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.Date.Month == month && e.Date.Year == year)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }
    }
}
