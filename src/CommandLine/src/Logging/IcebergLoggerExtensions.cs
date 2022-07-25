using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Iceberg.CommandLine.Logging;

internal static class IcebergLoggerExtensions
{
    public static ILoggingBuilder AddIcebergLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, IcebergLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <IcebergLoggerConfiguration, IcebergLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddIcebergLogger(
        this ILoggingBuilder builder,
        Action<IcebergLoggerConfiguration> configure)
    {
        builder.AddIcebergLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
