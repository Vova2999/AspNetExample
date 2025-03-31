﻿using Microsoft.AspNetCore.Identity;

namespace AspNetExample.Extensions;

public static class IdentityErrorExtensions
{
    public static string JoinErrors(
        this IEnumerable<IdentityError> errors,
        string? errorsSeparator = "\n")
    {
        return string.Join(
            errorsSeparator,
            errors.Select(error => error.Description));
    }
}