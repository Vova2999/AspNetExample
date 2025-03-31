﻿using System.Diagnostics.CodeAnalysis;

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

    public static string? NullIfEmpty(this string str)
    {
        return string.IsNullOrEmpty(str) ? null : str;
    }

    public static string EmptyIfNull(this string? str)
    {
        return str ?? string.Empty;
    }

    public static bool IsEquals(this string? source, string? searchString)
    {
        return string.Equals(source, searchString, StringComparison.Ordinal);
    }

    public static bool IsEqualsIgnoreCase(this string? source, string? searchString)
    {
        return string.Equals(source, searchString, StringComparison.OrdinalIgnoreCase);
    }

    public static TResult[] SplitAndParse<TResult>(
        this string value,
        char separator,
        TryParseHandler<TResult> tryParse)
    {
        return value.Split(separator).SafeParse(tryParse).ToArray();
    }

    public static TResult?[] SplitAndParseWithDefaults<TResult>(
        this string value,
        char separator,
        TryParseHandler<TResult> tryParse)
    {
        return value.Split(separator).SafeParseWithDefaults(tryParse).ToArray();
    }

    public static IEnumerable<TResult> SafeParse<TResult>(
        this IEnumerable<string> values,
        TryParseHandler<TResult> tryParse)
    {
        foreach (var value in values)
        {
            if (tryParse(value, out var result))
                yield return result;
        }
    }

    public static IEnumerable<TResult?> SafeParseWithDefaults<TResult>(
        this IEnumerable<string> values,
        TryParseHandler<TResult> tryParse)
    {
        foreach (var value in values)
        {
            if (tryParse(value, out var result))
                yield return result;
            else
                yield return default;
        }
    }

    public delegate bool TryParseHandler<T>(string value, out T result);
}