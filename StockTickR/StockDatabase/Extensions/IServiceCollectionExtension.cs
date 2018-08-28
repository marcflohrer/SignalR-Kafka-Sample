using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockDatabase.Repositories.Interfaces;

namespace StockDatabase.Extensions {
    public static class IServiceCollectionExtension {
        public static void AddDbContextFactory<DataContext> (this IServiceCollection services, string connectionString)
        where DataContext : DbContext {
            services.AddScoped<Func<DataContext>> ((ctx) => {
                var options = new DbContextOptionsBuilder<DataContext> ()
                    .UseSqlServer (connectionString)
                    .Options;

                return () => (DataContext) Activator.CreateInstance (typeof (DataContext), options);
            });
        }

        public static void AddSqlTableDependency<T> (this IServiceCollection services)
        where T : IDatabaseSubscription {
            services.AddScoped<Func<T>> ((ctx) => {
                return () => (T) Activator.CreateInstance (typeof (T));
            });
        }
    }
}