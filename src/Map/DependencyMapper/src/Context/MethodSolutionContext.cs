using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Iceberg.Map.DependencyMapper.Context;

public sealed partial class MethodSolutionContext : BaseMemberSolutionContext<MethodDeclarationSyntax>
{
    private readonly ILogger _logger;

    public MethodSolutionContext(ILogger logger, Solution solution) : base(logger, solution)
    {
        _logger = logger;
    }

    public MethodSolutionContext(ILoggerFactory loggerFactory, Solution solution) 
        : base(loggerFactory, solution)
    {
        _logger = loggerFactory.CreateLogger<MethodSolutionContext>();
    }

    public override async IAsyncEnumerable<EntryPoint<MethodDeclarationSyntax>> FindDependencyEntryPoints(
        EntryPoint<MethodDeclarationSyntax> entryPoint,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (entryPoint.Symbol is null)
        {
            Log.NoDeclaredSymbol(_logger);
            yield break;
        }

        var methodSymbolInfo = entryPoint.SyntaxNode.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(node => entryPoint.SemanticModel.GetSymbolInfo(node))
            .Where(symbolInfo => symbolInfo.Symbol is not null
                && symbolInfo.Symbol.Kind == SymbolKind.Method
                && IsAssemblyContainedInSolution(symbolInfo.Symbol.ContainingAssembly.Name));

        var abstractOrInterfaceMethodSymbolInfo = methodSymbolInfo
            .Where(symbolInfo => symbolInfo.Symbol!.IsAbstract
                || symbolInfo.Symbol!.IsVirtual);

        var concreteMethodSymbolInfo = methodSymbolInfo
            .Except(abstractOrInterfaceMethodSymbolInfo);

        foreach (var symbolInfo in abstractOrInterfaceMethodSymbolInfo)
        {
            var implementationSymbolsTask = FindImplementations(symbolInfo.Symbol!, cancellationToken);
            var overrideSymbolsTask = FindOverrides(symbolInfo.Symbol!, cancellationToken);

            var implementationSymbols = await implementationSymbolsTask;
            var overrideSymbols = await overrideSymbolsTask;

            await foreach (var dependencyEntryPoint in ConstructEntryPoints(
                new[] { implementationSymbols, overrideSymbols}.SelectMany(symbol => symbol)))
            {
                yield return dependencyEntryPoint;
            }
        }

        await foreach (var dependencyEntryPoint in ConstructEntryPoints(
            concreteMethodSymbolInfo.Select(symbolInfo => symbolInfo.Symbol!)))
        {
            yield return dependencyEntryPoint;
        }

        yield break;
    }

    private async Task<IEnumerable<ISymbol>> FindImplementations(
        ISymbol symbol,
        CancellationToken cancellationToken = default)
    {
        Log.FindingImplementations(_logger, symbol.Name);
        return await SymbolFinder.FindImplementationsAsync(symbol, Solution, cancellationToken: cancellationToken);
    }

    private async Task<IEnumerable<ISymbol>> FindOverrides(
        ISymbol symbol, 
        CancellationToken cancellationToken = default)
    {
        Log.FindingOverrides(_logger, symbol.Name);
        return await SymbolFinder.FindOverridesAsync(symbol, Solution, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<EntryPoint<MethodDeclarationSyntax>>> FindMethodEntryPoints(
        string className, 
        string methodName,
        CancellationToken cancellationToken = default)
    {
        Func<MethodDeclarationSyntax, SemanticModel, bool> predicate =
            (MethodDeclarationSyntax syntax, SemanticModel model) =>
            {
                var methodSymbol = model.GetDeclaredSymbol(syntax);
                if (methodSymbol == null
                    || !string.Equals(methodSymbol.Name, methodName, StringComparison.OrdinalIgnoreCase)
                    || syntax.Parent == null)
                {
                    return false;
                }

                var parent = syntax.Parent;
                if (parent is InterfaceDeclarationSyntax)
                {
                    return false;
                }

                var classSymbol = model.GetDeclaredSymbol(parent);

                return classSymbol != null
                    && string.Equals(classSymbol.Name, className, StringComparison.OrdinalIgnoreCase);
            };

        var entryPointMatches = await FindEntryPoints(predicate, cancellationToken);

        return entryPointMatches.DistinctBy(x => x.DisplayName);
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Finding implementations for {SymbolName}.", EventName = "FindingImplementations")]
        public static partial void FindingImplementations(ILogger logger, string symbolName);

        [LoggerMessage(2, LogLevel.Information, "Finding overrides for {SymbolName}.", EventName = "FindingOverrides")]
        public static partial void FindingOverrides(ILogger logger, string symbolName);

        [LoggerMessage(3, LogLevel.Information, "No declared symbol for entry point. Skipping.", EventName = "NoDeclaredSymbol")]
        public static partial void NoDeclaredSymbol(ILogger logger);
    }
}
