using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Iceberg.CommandLine.Logging;

[ProviderAlias("Iceberg")]
internal sealed class IcebergLoggerProvider : ILoggerProvider
{
    private readonly IDisposable _onChangeToken;
    private IcebergLoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, IcebergLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public IcebergLoggerProvider(IOptionsMonitor<IcebergLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new IcebergLogger(name, GetCurrentConfig));

    private IcebergLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken.Dispose();
    }
}
