using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

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

        if (syntaxContext.TargetNode is not ClassDeclarationSyntax classDeclaration)
            return null;

        if (!classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
            return null;

        if (
            syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken)
            is not { } classSymbol
        )
            return null;

        var wellKnownTypes = WellKnownTypes.WellKnownTypes.GetOrCreate(
            syntaxContext.SemanticModel.Compilation
        );

        if (
            classSymbol
                .GetAttributes()
                .FirstOrDefault(attr =>
                    attr.AttributeClass is not null
                    && wellKnownTypes.IsType(
                        attr.AttributeClass,
                        WellKnownType.DynamoMapper_Runtime_DynamoMapperAttribute
                    )
                )
            is not { } mapperAttribute
        )
            return null;

        var mapperOptions = mapperAttribute.PopulateOptions<MapperOptions>();

        var context = new GeneratorContext(
            syntaxContext,
            wellKnownTypes,
            mapperOptions,
            cancellationToken
        );

        return MapperInfo.Create(classSymbol, context);
    }
}
