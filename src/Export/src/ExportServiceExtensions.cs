using Microsoft.Extensions.DependencyInjection;

namespace Iceberg.Export;

public static class ExportServiceExtensions
{
    public static IServiceCollection RegisterExportServices(this IServiceCollection services)
    {
        services.AddSingleton<IDGMLBuilderService, DGMLBuilderService>();

        return services;
    }
}
