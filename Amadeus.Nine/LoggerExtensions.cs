using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Amadeus.Nine;

[SuppressMessage(
    "Performance",
    "CA1848:Use the LoggerMessage delegates",
    Justification = "it's not that big a deal")]
public static class LoggerExtensions
{
    public static T Info<T>(
        this ILogger logger,
        T item)
    {
        logger.LogInformation("{@Item}", item);
        return item;
    }

    public static T Info<T>(
        this ILogger logger,
        T item,
        string message)
    {
        logger.LogInformation(message);
        return item;
    }

    public static T Info<T>(
        this ILogger logger,
        T item,
        string message,
        params object[] args)
    {
        logger.LogInformation(message, args);
        return item;
    }

    public static T Debug<T>(
        this ILogger logger,
        T item)
    {
        logger.LogDebug("{@Item}", item);
        return item;
    }

    public static T Debug<T>(
        this ILogger logger,
        T item,
        string message)
    {
        logger.LogDebug(message);
        return item;
    }

    public static T Debug<T>(
        this ILogger logger,
        T item,
        string message,
        params object[] args)
    {
        logger.LogDebug(message, args);
        return item;
    }

    public static T Warning<T>(
        this ILogger logger,
        T item)
    {
        logger.LogWarning("{@Item}", item);
        return item;
    }

    public static T Warning<T>(
        this ILogger logger,
        T item,
        string message)
    {
        logger.LogWarning(message);
        return item;
    }

    public static T Warning<T>(
        this ILogger logger,
        T item,
        string message,
        params object[] args)
    {
        logger.LogWarning(message, args);
        return item;
    }

    public static T Error<T>(
        this ILogger logger,
        Exception ex,
        T item)
    {
        logger.LogError(ex, "{@Item}", item);
        return item;
    }

    public static T Error<T>(
        this ILogger logger,
        Exception ex,
        T item,
        string message)
    {
        logger.LogError(ex, message);
        return item;
    }

    public static T Error<T>(
        this ILogger logger,
        Exception ex,
        T item,
        string message,
        params object[] args)
    {
        logger.LogError(ex, message, args);
        return item;
    }
}
