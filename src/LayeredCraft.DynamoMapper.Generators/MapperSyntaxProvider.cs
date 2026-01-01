using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;
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

        var mapperResult = MapperClassInfo.CreateAndResolveModelType(classDeclaration, context);
        if (mapperResult is null)
            return null;

        var (mapperClassInfo, modelTypeSymbol) = mapperResult.Value;

        var (modelClassInfo, diagnosticInfos) = ModelClassInfo.Create(modelTypeSymbol, context);

        return new MapperInfo(mapperClassInfo, modelClassInfo, diagnosticInfos.ToEquatableArray());
    }
}
