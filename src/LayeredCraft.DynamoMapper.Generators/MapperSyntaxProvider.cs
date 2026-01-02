using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoMapper.Generator;

internal static class MapperSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    internal static MapperInfo? Transformer(
        GeneratorAttributeSyntaxContext syntaxContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = new GeneratorContext(syntaxContext, cancellationToken);

        if (syntaxContext.TargetNode is not ClassDeclarationSyntax classDeclaration)
            return null;

        if (!classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
            return null;

        return MapperInfo.Create(classDeclaration, context);
    }
}
