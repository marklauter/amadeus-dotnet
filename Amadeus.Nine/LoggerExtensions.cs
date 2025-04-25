using Microsoft.Extensions.Logging;

namespace Amadeus.Nine;

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
}
