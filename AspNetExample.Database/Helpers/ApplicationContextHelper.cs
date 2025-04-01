using AspNetExample.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Database.Helpers;

public static class ApplicationContextHelper
{
    public static DbContextOptions<ApplicationContext> BuildOptions(string connectionString)
    {
        return new DbContextOptionsBuilder<ApplicationContext>().UseNpgsql(connectionString).Options;
    }
}