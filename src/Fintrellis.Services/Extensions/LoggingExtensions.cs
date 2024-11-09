using Microsoft.Extensions.Logging;

namespace Fintrellis.Services.Extensions
{
    public static class LoggingExtensions
    {
        private static Action<ILogger, string, Exception> LogErrorMessageDelegate { get; } =
            LoggerMessage.Define<string>(LogLevel.Error, eventId:
                new EventId(id: 0, name: "Fintrellis.Services-ErrorLog"), formatString: "{message}");

        public static void LogErrorMessage(this ILogger logger, string message, Exception ex) => LogErrorMessageDelegate(logger, message, ex);
    }
}
