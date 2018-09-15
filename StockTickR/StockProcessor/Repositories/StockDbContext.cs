using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StockProcessor.Models;

namespace StockProcessor.Repositories {
    public class StockDbContext : DbContext {
        internal DbSet<Stock> Stocks {
            get;
            set;
        }
        public IConfigurationRoot Configuration {
            get;
        }
        public string DefaultSchema = "dbs";

        public StockDbContext (DbContextOptions options, IConfigurationRoot configuration) : base (options) {
            Configuration = configuration;
        }

        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema (DefaultSchema);

            modelBuilder.Entity<Stock> (e => {
                e.Property (f => f.Id).ValueGeneratedOnAdd ();
                e.Property (b => b.Symbol).HasColumnType ("varchar(256)");
                e.Property (b => b.Price).HasColumnType ("decimal(10, 2)");
                e.Property (b => b.DayLow).HasColumnType ("decimal(10, 2)");
                e.Property (b => b.DayOpen).HasColumnType ("decimal(10, 2)");
                e.Property (b => b.DayHigh).HasColumnType ("decimal(10, 2)");
                e.Property (b => b.LastChange).HasColumnType ("decimal(10, 2)");
                e.Property (b => b.UpdateTime).HasColumnType ("datetime2(7)");
            });
        }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder
                .UseSqlServer (string.Format (Configuration.GetConnectionString ("DefaultConnection"), Configuration["SA_PASSWORD"]));
        }

    }
}