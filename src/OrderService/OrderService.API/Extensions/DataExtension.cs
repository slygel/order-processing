using System;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure;

namespace OrderService.API.Extensions;

public static class DataExtension
{
    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
    }
}
