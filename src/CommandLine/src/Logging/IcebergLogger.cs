using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine.Logging;

internal sealed class IcebergLogger : ILogger
{
    private readonly string _name;
    private readonly Func<IcebergLoggerConfiguration> _getCurrentConfig;

    public IcebergLogger(string name, Func<IcebergLoggerConfiguration> getCurrentConfig)
    {
        _name = name;
        _getCurrentConfig = getCurrentConfig;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _getCurrentConfig().LogLevels.Contains(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        Console.WriteLine($"{formatter(state, exception)}");
    }
}
