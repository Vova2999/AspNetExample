using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetExample.Database.Context.Factory;

public class ApplicationContextFactory : IApplicationContextFactory
{
    private readonly DbContextOptions<ApplicationContext> _dbContextOptions;
    private readonly ILogger<ApplicationContext> _logger;

    public ApplicationContextFactory(
        DbContextOptions<ApplicationContext> dbContextOptions,
        ILogger<ApplicationContext> logger)
    {
        _dbContextOptions = dbContextOptions;
        _logger = logger;
    }

    public ApplicationContext Create()
    {
        return new ApplicationContext(_dbContextOptions, _logger);
    }
}