using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoMapper.Generator;

public readonly record struct MapperInfo;

public static class MapperSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    internal static MapperInfo? Transformer(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    ) => new MapperInfo();
}
