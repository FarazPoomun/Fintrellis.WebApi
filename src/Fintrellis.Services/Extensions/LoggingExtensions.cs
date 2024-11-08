using Microsoft.Extensions.Logging;

namespace Fintrellis.Services.Extensions
{
    public static class LoggingExtensions
    {
        private static Action<ILogger, string, Exception?> LogInfoMessageDelegate { get; } =
        LoggerMessage.Define<string>(LogLevel.Information, eventId:
            new EventId(id: 0, name: "Fintrellis.Services-InfoLog"), formatString: "{message}");

        private static Action<ILogger, string, Exception?> LogWarningMessageDelegate { get; } =
            LoggerMessage.Define<string>(LogLevel.Warning, eventId:
                new EventId(id: 0, name: "Fintrellis.Services-WarningLog"), formatString: "{message}");

        private static Action<ILogger, string, Exception> LogErrorMessageDelegate { get; } =
            LoggerMessage.Define<string>(LogLevel.Error, eventId:
                new EventId(id: 0, name: "Fintrellis.Services-ErrorLog"), formatString: "{message}");

        public static void LogInfoMessage(this ILogger logger, string message) => LogInfoMessageDelegate(logger, message, null);

        public static void LogWarningMessage(this ILogger logger, string message) => LogWarningMessageDelegate(logger, message, null);

        public static void LogErrorMessage(this ILogger logger, string message, Exception ex) => LogErrorMessageDelegate(logger, message, ex);
    }
}
