using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine.Logging;

internal class IcebergLoggerConfiguration
{
    public IEnumerable<LogLevel> LogLevels { get; set; } = new[]
    {
        LogLevel.Information
    };
}
