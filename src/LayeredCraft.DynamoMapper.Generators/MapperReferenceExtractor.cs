using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator;

/// <summary>
///     Lightweight extractor that collects basic mapper reference information
///     without performing full analysis. Used to build the MapperRegistry.
/// </summary>
internal static class MapperReferenceExtractor
{
    private const string ToMethodPrefix = "To";
    private const string FromMethodPrefix = "From";

    /// <summary>
    ///     Extracts mapper reference information from a class marked with [DynamoMapper].
    ///     Returns a tuple of (ModelTypeFullyQualifiedName, MapperReference) for registry building.
    /// </summary>
    internal static (string ModelTypeFqn, MapperReference Reference)? Extract(
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

        var attributes = classSymbol.GetAttributes();

        if (
            !attributes.Any(attr =>
                attr.AttributeClass is not null
                && wellKnownTypes.IsType(
                    attr.AttributeClass,
                    WellKnownType.DynamoMapper_Runtime_DynamoMapperAttribute
                )
            )
        )
            return null;

        // Find ToItem and FromItem methods to determine model type
        var methods = classSymbol.GetMembers().OfType<IMethodSymbol>().ToArray();

        var toItemMethod = methods.FirstOrDefault(m => IsToMethod(m, wellKnownTypes));
        var fromItemMethod = methods.FirstOrDefault(m => IsFromMethod(m, wellKnownTypes));

        if (toItemMethod is null && fromItemMethod is null)
            return null;

        // Get the model type from the method signatures
        var modelType = toItemMethod?.Parameters[0].Type ?? fromItemMethod?.ReturnType;
        if (modelType is null)
            return null;

        var modelTypeFqn = modelType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var mapperFqn = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var reference = new MapperReference(
            mapperFqn,
            HasToItemMethod: toItemMethod is not null,
            HasFromItemMethod: fromItemMethod is not null
        );

        return (modelTypeFqn, reference);
    }

    private static bool IsToMethod(
        IMethodSymbol method,
        WellKnownTypes.WellKnownTypes wellKnownTypes
    ) =>
        method.Name.StartsWith(ToMethodPrefix)
        && method
            is { IsPartialDefinition: true, PartialImplementationPart: null, Parameters.Length: 1 }
        && IsAttributeValueDictionary(method.ReturnType, wellKnownTypes);

    private static bool IsFromMethod(
        IMethodSymbol method,
        WellKnownTypes.WellKnownTypes wellKnownTypes
    ) =>
        method.Name.StartsWith(FromMethodPrefix)
        && method
            is { IsPartialDefinition: true, PartialImplementationPart: null, Parameters.Length: 1 }
        && IsAttributeValueDictionary(method.Parameters[0].Type, wellKnownTypes);

    private static bool IsAttributeValueDictionary(
        ITypeSymbol type,
        WellKnownTypes.WellKnownTypes wellKnownTypes
    ) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType
        && wellKnownTypes.IsType(
            namedType.ConstructedFrom,
            WellKnownType.System_Collections_Generic_Dictionary_2
        )
        && namedType.TypeArguments.Length == 2
        && wellKnownTypes.IsType(namedType.TypeArguments[0], WellKnownType.System_String)
        && wellKnownTypes.IsType(
            namedType.TypeArguments[1],
            WellKnownType.Amazon_DynamoDBv2_Model_AttributeValue
        );
}
