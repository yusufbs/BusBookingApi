using BusBooking.Application.Common.Interfaces;
using BusBooking.Infrastructure.Data;
using BusBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=BusBookingDb;Trusted_Connection=True;TrustServerCertificate=True;";

        // High-performance pooled DbContext
        services.AddDbContextPool<BusBookingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(BusBookingDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IBusBookingDbContext>(sp => sp.GetRequiredService<BusBookingDbContext>());

        // Dapper Connection Factory
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

        // JWT Token Generator
        services.AddSingleton<IJwtTokenService, Services.JwtTokenService>();

        // .NET 10 HybridCache registration
        #pragma warning disable EXTEXP0018 // HybridCache is preview in early packages
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new()
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });
        #pragma warning restore EXTEXP0018

        return services;
    }
}
