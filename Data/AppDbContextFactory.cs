using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WishListAPI.Data;

// Usado pelo `dotnet ef` em design-time para gerar migrations sem precisar subir o host do ASP.NET.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            config.GetConnectionString("DefaultConnection") ??
            "Host=localhost;Port=5432;Database=design_time;Username=postgres;Password=postgres;Ssl Mode=Disable";

        cs = DbConnectionString.NormalizePostgres(cs);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new AppDbContext(options);
    }
}

