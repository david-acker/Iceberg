using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper;

public class EntryPoint<T> : IEqualityComparer<EntryPoint<T>> where T : MemberDeclarationSyntax
{
    public T SyntaxNode { get; init; }
    public SemanticModel SemanticModel { get; init; }

    public ISymbol? Symbol { get; init; } 
    public string DisplayName { get; init; }

    internal EntryPoint(T syntaxNode, SemanticModel semanticModel)
    {
        SyntaxNode = syntaxNode;
        SemanticModel = semanticModel;

        Symbol = SemanticModel.GetDeclaredSymbol(syntaxNode);
        DisplayName = Symbol?.ToDisplayString() ?? string.Empty;
    }

    public bool Equals(EntryPoint<T>? x, EntryPoint<T>? y)
    {
        return x?.DisplayName == y?.DisplayName;
    }

    public int GetHashCode([DisallowNull] EntryPoint<T> obj)
    {
        return obj.DisplayName.GetHashCode();
    }
}
