using System.Diagnostics.CodeAnalysis;

namespace AspNetExample.Common.Extensions;

public static class StringExtensions
{
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
	{
		return string.IsNullOrEmpty(str);
	}

	public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	public static bool IsSignificant([NotNullWhen(true)] this string? str)
	{
		return !string.IsNullOrEmpty(str);
	}
}