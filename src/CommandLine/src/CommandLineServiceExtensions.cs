using Iceberg.CommandLine.Commands;
using Iceberg.CommandLine.Commands.Load;
using Iceberg.CommandLine.Commands.Load.Options;
using Iceberg.CommandLine.Commands.Load.Services;
using Iceberg.CommandLine.Commands.Map;
using Iceberg.CommandLine.Commands.Map.Options;
using Iceberg.CommandLine.Commands.Unload;
using Iceberg.CommandLine.Commands.View;
using Iceberg.CommandLine.Commands.View.Services;
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

        services.AddScoped<IFileService, FileService>();
        
        // TODO: Use assembly scanning.
        services.AddTransient<ICommand, LoadCommand>();
        services.AddTransient<ILoadCommandHandler, LoadCommandHandler>();
        services.AddTransient<IPathOptionParser, PathOptionParser>();

        services.AddTransient<ICommand, UnloadCommand>();
        services.AddTransient<IUnloadCommandHandler, UnloadCommandHandler>();
        
        services.AddTransient<ICommand, MapCommand>();
        services.AddTransient<IMapCommandHandler, MapCommandHandler>();
        services.AddTransient<IClassOptionParser, ClassOptionParser>();
        services.AddTransient<IDistanceOptionParser, DistanceOptionParser>();
        services.AddTransient<IFlowOptionParser, FlowOptionParser>();
        services.AddTransient<IMethodOptionParser, MethodOptionParser>();
        services.AddTransient<IProjectOptionParser, ProjectOptionParser>();
        
        services.AddTransient<ICommand, ViewCommand>();
        services.AddTransient<IViewCommandHandler, ViewCommandHandler>();
        // TODO: Register the appropiate implementation based on the detected OS.
        services.AddScoped<IProcessService, WindowsProcessService>();

        services.AddScoped<IIcebergSessionHandler, IcebergSessionHandler>();

        return services;
    }
}
