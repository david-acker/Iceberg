using Iceberg.CommandLine.Commands;
using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace Iceberg.CommandLine;

public static class CommandLineServiceExtensions
{
    public static IServiceCollection RegisterCommandLineServices(this IServiceCollection services)
    {
        services.AddScoped<IIcebergSessionContext, IcebergSessionContext>();

        services.AddTransient<ISolutionWrapper, SolutionWrapper>(serviceProvider =>
        {
            var sessionContext = serviceProvider.GetRequiredService<IIcebergSessionContext>();

            return new SolutionWrapper(sessionContext.Solution!);
        });

        services.AddTransient<ICommand, LoadCommand>();
        services.AddTransient<ICommand, UnloadCommand>();
        services.AddTransient<ICommand, MapCommand>();
        services.AddTransient<ICommand, ViewCommand>();

        services.AddScoped<IIcebergSessionHandler, IcebergSessionHandler>();

        return services;
    }
}
