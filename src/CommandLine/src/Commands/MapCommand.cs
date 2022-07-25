using Iceberg.Export;
using Iceberg.Map.DependencyMapper;
using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;

namespace Iceberg.CommandLine.Commands;

internal class MapCommand : ICommand
{
    private const string _name = "map";
    private const string _description = "Map the dependencies for the specified entry point.";

    public Command Value { get; }

    public MapCommand(
        IIcebergSessionContext context, 
        ILogger<MapCommand> logger,
        ILoggerFactory loggerFactory,
        IServiceProvider provider)
    {
        Value = CreateCommand(context, logger, loggerFactory, provider);
    }

    public static Command CreateCommand(
        IIcebergSessionContext context, 
        ILogger<MapCommand> logger,
        ILoggerFactory loggerFactory,
        IServiceProvider provider)
    {
        var command = new Command(_name, _description);

        var flowOption = GetFlowOption();
        command.Add(flowOption);

        var methodOption = GetMethodOption();
        command.Add(methodOption);

        var classOption = GetClassOption();
        command.Add(classOption);

        var projectOption = GetProjectOption();
        command.Add(projectOption);

        var distanceOption = GetMaximumDistanceOption();
        command.Add(distanceOption);

        command.SetHandler(async (flowOptionValue, methodOptionValue, classOptionValue, projectOptionValue, distanceOptionValue) =>
        {
            if (!context.IsSolutionLoaded
                || context.Solution is null)
            {
                Console.WriteLine("No solution loaded.");
                return;
            }

            var methodSolutionContext = provider.GetService<IMethodSolutionContext>()!;

            var cancellationTokenSource = new CancellationTokenSource();

            var matchingMethodEntryPoints =
                await methodSolutionContext.FindMethodEntryPoints(
                    classOptionValue,
                    methodOptionValue,
                    projectOptionValue,
                    cancellationTokenSource.Token);

            if (!matchingMethodEntryPoints.Any())
            {
                Console.WriteLine("No matching entry points found.");
                return;
            }

            var methodDependencyMapper = provider.GetService<IMethodDependencyMapper>()!;

            var methodDependencyMap = 
                await GetMethodDependencyMap(
                    flowOptionValue,
                    methodDependencyMapper, 
                    methodSolutionContext, 
                    matchingMethodEntryPoints, 
                    cancellationTokenSource.Token);

            if (distanceOptionValue.HasValue)
            {
                var entryPointMethodMetadata = methodDependencyMap.Keys
                    .Where(x =>
                        methodOptionValue is null || x.MethodName.StartsWith(methodOptionValue)
                        && x.ClassName.StartsWith(classOptionValue));

                methodDependencyMap = GetTrimmedDependencyMap(methodDependencyMap, entryPointMethodMetadata, distanceOptionValue.Value);
            }

            // TODO: Export this to a more accessible place while testing.
            // Currently written to Iceberg\src\CommandLine\src\bin\Debug\net6.0
            var outputFileName = "dependency_map.gen.dgml";

            var dgmlBuilderService = provider.GetService<IDGMLBuilderService>()!;

            var dependencyMapName =
                string.IsNullOrWhiteSpace(methodOptionValue)
                    ? classOptionValue
                    : $"{classOptionValue}.{methodOptionValue}";

            // TODO: Don't overwrite generated maps.

            var exportedDGML = dgmlBuilderService.ExportDependencyMap(dependencyMapName, methodDependencyMap);

            exportedDGML.Save(outputFileName);
            context.LastGeneratedDependencyMap = outputFileName;

            logger.LogInformation($"Saved generated dependency map to {outputFileName}");
        },
        flowOption, methodOption, classOption, projectOption, distanceOption);

        return command;
    }

    private static async Task<MethodDependencyMap> GetMethodDependencyMap(
        MappingFlow mappingFlow,
        IMethodDependencyMapper methodDependencyMapper,
        IMethodSolutionContext solutionContext, 
        IEnumerable<IEntryPoint<MethodDeclarationSyntax>> entryPoints,
        CancellationToken cancellationToken)
    {
        // TODO: Handle zero or multiple matching entry points based on additional input from the user.
        // TODO: Add a separate subcommand or options to specify that an entire class should be mapped.

        if (mappingFlow == MappingFlow.Downstream)
        {
            return await methodDependencyMapper.MapDownstream(
                solutionContext,
                entryPoints,
                cancellationToken);
        }
        else if (mappingFlow == MappingFlow.Upstream)
        {
            return await methodDependencyMapper.MapUpstream(
                solutionContext,
                entryPoints,
                cancellationToken);
        }

        Debug.Assert(false, "Invalid Mapping Flow Option.");
        return null!;
    }

    // TODO: Handle this logic in the MapUpstream/MapDownstream methods in BaseMemberSolutionContext
    // so that dependencies past the maximum distance from the entry point aren't unnecessarily mapped.
    private static MethodDependencyMap GetTrimmedDependencyMap(
        MethodDependencyMap generatedMethodDependencyMap,
        IEnumerable<MethodMetadata> entryPointMethodMetadata,
        int maximumDegreeOfSeparation)
    {
        var trimmedMethodDependencyMap = new MethodDependencyMap();

        foreach (var methodMetadata in entryPointMethodMetadata)
        {
            Traverse(generatedMethodDependencyMap, trimmedMethodDependencyMap, methodMetadata, maximumDegreeOfSeparation);
        }

        return trimmedMethodDependencyMap;
    }

    private static void Traverse(
        MethodDependencyMap generatedMethodDependencyMap,
        MethodDependencyMap trimmedMethodDependencyMap,
        MethodMetadata entryPointMethodMetadata,
        int remainingDegreeOfSeparation)
    {
        if (trimmedMethodDependencyMap.ContainsKey(entryPointMethodMetadata))
            return;

        if (remainingDegreeOfSeparation < 1)
        {
            trimmedMethodDependencyMap[entryPointMethodMetadata] = new HashSet<MethodMetadata>();
            return;
        }

        trimmedMethodDependencyMap[entryPointMethodMetadata] = generatedMethodDependencyMap[entryPointMethodMetadata];

        foreach (var dependencyMethodMetadata in generatedMethodDependencyMap[entryPointMethodMetadata])
        {
            Traverse(generatedMethodDependencyMap, trimmedMethodDependencyMap, dependencyMethodMetadata, remainingDegreeOfSeparation);
        }
    }

    private static Option<MappingFlow> GetFlowOption()
    {
        return new Option<MappingFlow>(
            aliases: new[] { "--flow", "-f" },
            description: "The flow of the dependency map.",
            parseArgument: result =>
            {
                var flowName = result.Tokens.Single().Value.Trim();

                if (string.Equals(flowName, "downstream", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(flowName, "d", StringComparison.OrdinalIgnoreCase))
                {
                    return MappingFlow.Downstream;
                }

                if (string.Equals(flowName, "upstream", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(flowName, "u", StringComparison.OrdinalIgnoreCase))
                {
                    return MappingFlow.Upstream;
                }

                result.ErrorMessage = "Unrecognized mapping flow name. Please provide a valid flow name: downstream/d, upstream/u";
                return default;
            })
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = false
        };
    }

    private enum MappingFlow
    {
        Downstream,
        Upstream
    }

    private static Option<string> GetMethodOption()
    {
        return new Option<string>(
            aliases: new[] { "--method", "-m" },
            description: "The method name to use as the entry point.",
            parseArgument: result =>
            {
                if (!result.Tokens.Any())
                    return string.Empty;

                var methodName = result.Tokens.Single().Value;

                if (string.IsNullOrWhiteSpace(methodName))
                {
                    result.ErrorMessage = "The method name cannot be blank.";
                    return string.Empty;
                }

                return methodName;
            })
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = false
        };
    }

    private static Option<string> GetClassOption()
    {
        return new Option<string>(
            aliases: new[] { "--class", "-c" },
            description: "The class name to use as the entry point.",
            parseArgument: result =>
            {
                var className = result.Tokens.Single().Value;

                if (string.IsNullOrEmpty(className))
                {
                    result.ErrorMessage = "The class name cannot be blank.";
                    return string.Empty;
}

                return className;
            })
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = false
        };
    }

    private static Option<string> GetProjectOption()
    {
        return new Option<string>(
            aliases: new[] { "--project", "-p" },
            description: "The project where the entry point is located.",
            parseArgument: result =>
            {
                if (!result.Tokens.Any())
                    return string.Empty;

                var projectName = result.Tokens.Single().Value;

                if (string.IsNullOrWhiteSpace(projectName))
                {
                    result.ErrorMessage = "The project name cannot be blank.";
                    return string.Empty;
                }

                return projectName;
            })
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = false
        };
    }

    private static Option<int?> GetMaximumDistanceOption()
    {
        return new Option<int?>(
            aliases: new[] { "--distance", "-d" },
            description: "The maximum distance to map from the entry point.",
            parseArgument: result =>
            {
                if (!result.Tokens.Any())
                    return null;

                var maximumDistanceString = result.Tokens.Single().Value;

                if (!string.IsNullOrEmpty(maximumDistanceString)
                    || int.TryParse(maximumDistanceString, out int maximumDistance))
                {
                    result.ErrorMessage = "The maximum distance must be a valid integer greater than zero.";
                    return null;
                }

                if (maximumDistance < 1)
                {
                    result.ErrorMessage = "The maximum distance must be greater than zero.";
                    return null;
                }

                return maximumDistance;
            })
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = false
        };
    }
}
