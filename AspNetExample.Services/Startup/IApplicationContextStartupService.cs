﻿namespace AspNetExample.Services.Startup;

public interface IApplicationContextStartupService
{
    Task InitializeUsersAndRolesAsync();
}