using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Models;
using System;
using System.IO;

namespace PersonalFinanceManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "personal_finance.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food", Color = "#FF5733" },
                new Category { Id = 2, Name = "Transport", Color = "#33FF57" },
                new Category { Id = 3, Name = "Utilities", Color = "#3357FF" },
                new Category { Id = 4, Name = "Entertainment", Color = "#F333FF" },
                new Category { Id = 5, Name = "Health", Color = "#FF3333" }
            );
        }
    }
}
