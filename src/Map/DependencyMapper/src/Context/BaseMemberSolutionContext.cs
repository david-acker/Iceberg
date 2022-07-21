using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Iceberg.Map.DependencyMapper.Context;

public abstract partial class BaseMemberSolutionContext<T> where T : MemberDeclarationSyntax
{
    private readonly ILogger _logger;
    protected readonly ISolutionWrapper SolutionWrapper;

    private readonly IDictionary<string, Project> _projects = new Dictionary<string, Project>();

    public BaseMemberSolutionContext(ILoggerFactory loggerFactory, ISolutionWrapper solutionWrapper)
    {
        _logger = loggerFactory.CreateLogger<BaseMemberSolutionContext<T>>();
        SolutionWrapper = solutionWrapper;

        foreach (var project in SolutionWrapper.Projects)
        {
            _projects[project.AssemblyName] = project;
        }
    }

    public BaseMemberSolutionContext(ILogger logger, ISolutionWrapper solutionWrapper)
    {
        _logger = logger;
        SolutionWrapper = solutionWrapper;

        foreach (var project in SolutionWrapper.Projects)
        {
            _projects[project.AssemblyName] = project;
        }
    }

    public abstract IAsyncEnumerable<IEntryPoint<T>> FindDependencyEntryPoints(
        IEntryPoint<T> entryPoint,
        CancellationToken cancellationToken = default);

    protected bool IsAssemblyContainedInSolution(string assemblyName) => _projects.ContainsKey(assemblyName);

    protected async IAsyncEnumerable<IEntryPoint<T>> ConstructEntryPoints(
        IEnumerable<ISymbol> symbols,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) 
    {
        foreach (var symbol in symbols)
        {
            if (!symbol.DeclaringSyntaxReferences.Any())
            {
                Log.NoDeclaringSyntaxReferences(_logger, symbol.Name);
                continue;
            }

            // TODO: This should be supported eventually.
            if (symbol.DeclaringSyntaxReferences.Length > 1)
            {
                Log.MultipleDeclaringSyntaxReferences(_logger, symbol.Name);
                continue;
            }

            var declaringSyntaxNode = await symbol.DeclaringSyntaxReferences.First().GetSyntaxAsync(cancellationToken);
            if (declaringSyntaxNode is not T derivedDeclaringSyntaxNode)
            {
                Log.IncorrectSyntaxType(_logger, typeof(T).Name);
                continue;
            }

            if (!_projects.TryGetValue(symbol.ContainingAssembly.Name, out Project? containingProject))
            {
                Log.SymbolNotDeclaredInSolution(_logger, symbol.Name);
                continue;
            }

            if (!symbol.Locations.Any())
            {
                Log.SymbolHasNoLocations(_logger, symbol.Name);
                continue;
            }

            // TODO: This should be supported eventually.
            if (symbol.Locations.Length > 1)
            {
                Log.SymbolHasMultipleLocations(_logger, symbol.Name);
                continue;
            }

            var sourceTree = symbol.Locations.First().SourceTree;
            if (sourceTree is null)
            {
                Log.NullSourceTree(_logger);
                continue;
            }

            var document = containingProject!.GetDocument(sourceTree);
            if (document is null)
            {
                var projectName = containingProject.Name;
                var syntaxTreeFilePath = Path.GetFileName(sourceTree.FilePath);

                Log.ProjectDoesNotContainDocument(_logger, projectName, syntaxTreeFilePath);
                continue;
            }

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel is null)
            {
                Log.CouldNotGetSemanticModel(_logger, document.Name);
                continue;
            }

            yield return new EntryPoint<T>(derivedDeclaringSyntaxNode, semanticModel);
        }
    }

    public async Task<IEnumerable<IEntryPoint<T>>> FindEntryPoints(
        Func<T, SemanticModel, bool> predicate,
        string? projectName = null,
        CancellationToken cancellationToken = default)
    {
        var matchingEntryPoints = new ConcurrentBag<EntryPoint<T>>();

        await Parallel.ForEachAsync(GetDocumentsToSearch(projectName, cancellationToken),
            async (documentMetadata, cancellationToken) =>
            {
                var rootNode = await documentMetadata.SyntaxTree.GetRootAsync(cancellationToken);

                var matchingNodes = rootNode
                    .DescendantNodesAndSelf()
                    .OfType<T>()
                    .Where((node) => predicate(node, documentMetadata.SemanticModel));

                var entryPoints = matchingNodes
                    .Select(node => new EntryPoint<T>(node, documentMetadata.SemanticModel));

                foreach (var entryPoint in entryPoints)
                {
                    matchingEntryPoints.Add(entryPoint);
                }
            });

        return matchingEntryPoints;
    }

    private async IAsyncEnumerable<DocumentMetadata> GetDocumentsToSearch(
        string? projectName = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var projectsToSearch = projectName == null
            ? _projects.Values
            : _projects.Values.Where(p => p.Name.Equals(projectName));

        foreach (var project in projectsToSearch)
        {
            foreach (var document in project.Documents
                .Where(doc => doc.SupportsSyntaxTree && doc.SupportsSyntaxTree))
            {
                var syntaxTreeTask = document.GetSyntaxTreeAsync(cancellationToken);
                var semanticModelTask = document.GetSemanticModelAsync(cancellationToken);

                var syntaxTree = await syntaxTreeTask;
                var semanticModel = await semanticModelTask;

                if (syntaxTree != null && semanticModel != null)
                    yield return new DocumentMetadata(syntaxTree, semanticModel);
            }
        }
    }

    private class DocumentMetadata
    {
        public SyntaxTree SyntaxTree { get; init; }
        public SemanticModel SemanticModel { get; init; }

        public DocumentMetadata(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            SyntaxTree = syntaxTree;
            SemanticModel = semanticModel;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Symbol {SymbolName} has no declaring syntax references. Skipping.", EventName = "NoDeclaringSyntaxReferences")]
        public static partial void NoDeclaringSyntaxReferences(ILogger logger, string symbolName);

        [LoggerMessage(2, LogLevel.Information, "Symbol {SymbolName} has multiple declaring syntax references. Skipping.", EventName = "MultipleDeclaringSyntaxReferences")]
        public static partial void MultipleDeclaringSyntaxReferences(ILogger logger, string symbolName);

        [LoggerMessage(3, LogLevel.Information, "Declaring syntax was not of type {SyntaxTypeName}. Skipping.", EventName = "IncorrectDeclaringSyntaxType")]
        public static partial void IncorrectSyntaxType(ILogger logger, string syntaxTypeName);

        [LoggerMessage(4, LogLevel.Information, "Symbol {SymbolName} was not declared in the solution. Skipping.", EventName = "SymbolNotDeclaredInSolution")]
        public static partial void SymbolNotDeclaredInSolution(ILogger logger, string symbolName);

        [LoggerMessage(5, LogLevel.Information, "Symbol {SymbolName} has no locations. Skipping.", EventName = "SymboHasNotLocations")]
        public static partial void SymbolHasNoLocations(ILogger logger, string symbolName);

        [LoggerMessage(6, LogLevel.Information, "Symbol {SymbolName} has mutliple locations. Skipping.", EventName = "SymboHasMultipleLocations")]
        public static partial void SymbolHasMultipleLocations(ILogger logger, string symbolName);

        [LoggerMessage(7, LogLevel.Information, "Encountered a null source tree. Skipping.", EventName = "NullSourceTree")]
        public static partial void NullSourceTree(ILogger logger);

        [LoggerMessage(8, LogLevel.Information, "Project {ProjectName} does not contain Document {DocumentName}. Skipping.", EventName = "ProjectDoesNotContainDocument")]
        public static partial void ProjectDoesNotContainDocument(ILogger logger, string projectName, string documentName);

        [LoggerMessage(9, LogLevel.Information, "Could not get semantic model for Document {DocumentName}. Skipping.", EventName = "CouldNotGetSemanticModel")]
        public static partial void CouldNotGetSemanticModel(ILogger logger, string documentName);
    }
}
