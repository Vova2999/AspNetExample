﻿#pragma warning disable CS8618

namespace AspNetExample.Domain.Dtos;

public class ChangePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}