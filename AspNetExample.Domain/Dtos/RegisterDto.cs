﻿#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AspNetExample.Domain.Dtos;

public class RegisterDto
{
    public string Login { get; set; }
    public string Password { get; set; }
}