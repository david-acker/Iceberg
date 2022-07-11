using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iceberg.Map.DependencyMapper;

public sealed class MethodEntryPoint : EntryPoint<MethodDeclarationSyntax>
{
    internal MethodEntryPoint(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel) 
        : base(methodDeclarationSyntax, semanticModel)
    {
    }
}
