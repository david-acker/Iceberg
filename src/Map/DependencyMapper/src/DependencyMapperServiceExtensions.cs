using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.Selectors;
using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace Iceberg.Map.DependencyMapper;

public static class DependencyMapperServiceExtensions
{
    public static IServiceCollection RegisterDependencyMapperServices(this IServiceCollection services)
    {
        services.AddSingleton<ISymbolEqualityComparerWrapper, SymbolEqualityComparerWrapper>();
        services.AddSingleton<ISymbolFinderWrapper, SymbolFinderWrapper>();

        // TODO: Use assembly scanning.
        services.AddSingleton<IMethodSelector, AbstractOrVirtualMethodSelector>();
        services.AddSingleton<IMethodSelector, ConcreteMethodSelector>();
        services.AddSingleton<IMethodSelector, OverriddenMethodSelector>();

        services.AddTransient<IMethodSolutionContext, MethodSolutionContext>();
        services.AddTransient<IMethodDependencyMappingContext, MethodDependencyMappingContext>();
        services.AddScoped<IMethodDependencyMapper, MethodDependencyMapper>();

        return services;
    }
}
