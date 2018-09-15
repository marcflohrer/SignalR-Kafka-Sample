using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StockProcessor.Repositories.Interfaces;

namespace StockProcessor.Extensions {
    public static class ApplicationBuilderExtensions {
        public static void UseSqlTableDependency<T> (this IApplicationBuilder app, string connectionString)
        where T : IDatabaseSubscription {
            var serviceProvider = app.ApplicationServices;
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory> ();
            using (var scope = scopeFactory.CreateScope ()) {
                var subscription = (T) scope.ServiceProvider.GetRequiredService<T> ();
                if (subscription == null) {
                    throw new System.Exception ("Cannot resolve service: " + typeof (T));
                }
                subscription.Configure (connectionString);
            }
        }
    }
}