using System;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;

namespace ProductService.API.Extensions;

public static class DataExtension
{
    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
    }
}
