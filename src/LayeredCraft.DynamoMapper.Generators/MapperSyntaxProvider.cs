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

    /// <summary>
    ///     Transforms a syntax context into a MapperInfo with the provided registry.
    ///     This is the primary entry point used by the generator pipeline.
    /// </summary>
    internal static MapperInfo? TransformerWithRegistry(
        GeneratorAttributeSyntaxContext syntaxContext,
        MapperRegistry registry,
        CancellationToken cancellationToken
    ) =>
        TransformCore(syntaxContext, registry, cancellationToken);

    /// <summary>
    ///     Transforms a syntax context into a MapperInfo without a registry.
    ///     Used for backwards compatibility and testing.
    /// </summary>
    internal static MapperInfo? Transformer(
        GeneratorAttributeSyntaxContext syntaxContext,
        CancellationToken cancellationToken
    ) =>
        TransformCore(syntaxContext, null, cancellationToken);

    private static MapperInfo? TransformCore(
        GeneratorAttributeSyntaxContext syntaxContext,
        MapperRegistry? registry,
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

        var attributes = classSymbol.GetAttributes();

        if (
            attributes.FirstOrDefault(attr =>
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

        var fieldOptions = attributes
            .Where(attr =>
                attr.AttributeClass is not null
                && wellKnownTypes.IsType(
                    attr.AttributeClass,
                    WellKnownType.DynamoMapper_Runtime_DynamoFieldAttribute
                )
            )
            .Select(attr => attr.PopulateOptions<DynamoFieldOptions>())
            .ToDictionary(fieldOption => fieldOption.MemberName);

        var ignoreOptions = attributes
            .Where(attr =>
                attr.AttributeClass is not null
                && wellKnownTypes.IsType(
                    attr.AttributeClass,
                    WellKnownType.DynamoMapper_Runtime_DynamoIgnoreAttribute
                )
            )
            .Select(attr => attr.PopulateOptions<DynamoIgnoreOptions>())
            .ToDictionary(ignoreOption => ignoreOption.MemberName);

        var context = new GeneratorContext(
            syntaxContext,
            wellKnownTypes,
            mapperOptions,
            fieldOptions,
            ignoreOptions,
            registry,
            cancellationToken
        );

        return MapperInfo.Create(classSymbol, context);
    }
}
