﻿// ReSharper disable ClassNeverInstantiated.Global

using Microsoft.Extensions.Logging;

namespace AspNetExample.Database.Context.Factory;

public class ApplicationContextFactory : IApplicationContextFactory
{
	private readonly ILogger<ApplicationContext> _logger;

	public ApplicationContextFactory(ILogger<ApplicationContext> logger)
	{
		_logger = logger;
	}

	public ApplicationContext Create()
	{
		return new ApplicationContext(_logger);
	}
}