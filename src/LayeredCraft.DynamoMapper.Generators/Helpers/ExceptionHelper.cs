namespace DynamoMapper.Generator.Helpers;

/// <summary>Helper class for formatting exceptions for diagnostic reporting.</summary>
internal static class ExceptionHelper
{
    private const string NewLine = "\n";

    /// <summary>Formats an exception for diagnostic reporting. Includes stack trace only in DEBUG builds.</summary>
    /// <param name="ex">The exception to format</param>
    /// <returns>Formatted exception message</returns>
    internal static string FormatExceptionMessage(Exception ex)
    {
#if DEBUG
        return $"{ex.GetType().Name}: {ex.Message}{NewLine}Stack trace:{NewLine}{ex.StackTrace}";
#else
        return $"{ex.GetType().Name}: {ex.Message}";
#endif
    }

    /// <summary>Formats an exception with inner exception details.</summary>
    /// <param name="ex">The exception to format</param>
    /// <returns>Formatted exception message with inner exception details</returns>
    internal static string FormatExceptionWithInner(Exception ex)
    {
        var message = FormatExceptionMessage(ex);

        if (ex.InnerException != null)
            message += $"{NewLine}Inner exception: {FormatExceptionMessage(ex.InnerException)}";

        return message;
    }
}
