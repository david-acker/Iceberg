using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Iceberg.CommandLine.Logging;

internal class IcebergLoggingFormatter : ConsoleFormatter
{
    public IcebergLoggingFormatter() : base(nameof(IcebergLoggingFormatter))
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry, 
        IExternalScopeProvider? scopeProvider, 
        TextWriter textWriter)
    {
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        if (message is null)
            return;

        textWriter.WriteLine(message);
    }
}
