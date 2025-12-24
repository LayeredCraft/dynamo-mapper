using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MapperResult = (
    DynamoMapper.Generator.MapperClassInfo MapperClass,
    Microsoft.CodeAnalysis.ITypeSymbol ModelType
);
using WellKnownType = DynamoMapper.Generator.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator;

internal readonly record struct MapperClassInfo(
    string Name,
    string FullyQualifiedType,
    string Accessibility,
    string Namespace,
    string? ToItemSignature,
    string? FromItemSignature
);

internal static class MapperClassInfoExtensions
{
    private const string ToItemMethodName = "ToItem";
    private const string FromItemMethodName = "FromItem";

    private static readonly SymbolDisplayFormat MethodFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters
            | SymbolDisplayMemberOptions.IncludeModifiers
            | SymbolDisplayMemberOptions.IncludeAccessibility
            | SymbolDisplayMemberOptions.IncludeType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType
            | SymbolDisplayParameterOptions.IncludeName
            | SymbolDisplayParameterOptions.IncludeParamsRefOut
    );

    extension(MapperClassInfo)
    {
        internal static (
            MapperClassInfo MapperClassInfo,
            ITypeSymbol ModelTypeSymbol
        )? CreateAndResolveModelType(
            ClassDeclarationSyntax classDeclaration,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            if (!classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
                return null;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (classSymbol is null)
                return null;

            /*
             * to determine what mappers to generate, we need to look for methods using these rules:
             * - `ToItem` -> method to map from POCO to AttributeValue
             * - `FromItem` -> method to map from AttributeValue to POCO
             *
             * Rules:
             * - at least one is needed, but both are not required
             * - the POCO type on both mapper methods must match.
             */

            // TODO: add diagnostic to warn about invalid mapper methods
            var methodSymbols = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(static m => m.IsPartialDefinition && m.PartialImplementationPart is null)
                .Where(m => HasSupportedSignature(m, context))
                .ToArray();

            var toItemMethod = methodSymbols.FirstOrDefault(static m =>
                string.Equals(m.Name, ToItemMethodName, StringComparison.Ordinal)
            );
            var fromItemMethod = methodSymbols.FirstOrDefault(static m =>
                string.Equals(m.Name, FromItemMethodName, StringComparison.Ordinal)
            );

            var (mapperClassInfo, modelTypeSymbol) = BuildMapperClassInfoCore(
                classSymbol,
                toItemMethod,
                fromItemMethod
            );

            return (mapperClassInfo, modelTypeSymbol);
        }
    }

    private static MapperResult BuildMapperClassInfoCore(
        INamedTypeSymbol classSymbol,
        IMethodSymbol? toItemMethod,
        IMethodSymbol? fromItemMethod
    )
    {
        var modelTypeSymbol = EnsurePocoTypesMatch(toItemMethod, fromItemMethod);
        var toItemSignature = GetMethodSignature(toItemMethod);
        var fromItemSignature = GetMethodSignature(fromItemMethod);

        var mapperClassInfo = new MapperClassInfo(
            classSymbol.Name,
            classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            classSymbol.DeclaredAccessibility.ToString(),
            classSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            toItemSignature,
            fromItemSignature
        );

        return (mapperClassInfo, modelTypeSymbol);
    }

    private static ITypeSymbol EnsurePocoTypesMatch(
        IMethodSymbol? toItemMethod,
        IMethodSymbol? fromItemMethod
    )
    {
        if (toItemMethod is null && fromItemMethod is null)
            // TODO: replace with diagnostic to warn about no mapper methods found
            throw new InvalidOperationException(
                $"Neither '{ToItemMethodName}' nor '{FromItemMethodName}' is present on the mapper class."
            );

        var toItemPocoType = toItemMethod?.Parameters[0].Type;
        var fromItemPocoType = fromItemMethod?.ReturnType;

        if (
            toItemPocoType is not null
            && fromItemPocoType is not null
            && !SymbolEqualityComparer.Default.Equals(toItemPocoType, fromItemPocoType)
        )
            // TODO: add propper diagnostic for ToItem and FromItem not using same POCO
            throw new InvalidOperationException(
                $"Mapper methods '{ToItemMethodName}' and '{FromItemMethodName}' must use the same POCO type."
            );

        return toItemPocoType ?? fromItemPocoType!;
    }

    private static bool HasSupportedSignature(IMethodSymbol method, GeneratorContext context)
    {
        context.ThrowIfCancellationRequested();

        if (method.Parameters.Length != 1)
            return false;

        if (string.Equals(method.Name, ToItemMethodName, StringComparison.Ordinal))
            return IsAttributeValueDictionary(method.ReturnType, context);

        if (string.Equals(method.Name, FromItemMethodName, StringComparison.Ordinal))
            return !method.ReturnsVoid
                && IsAttributeValueDictionary(method.Parameters[0].Type, context);

        return false;
    }

    private static bool IsAttributeValueDictionary(ITypeSymbol type, GeneratorContext context)
    {
        context.ThrowIfCancellationRequested();

        var dictionaryType = context.WellKnownTypes.Get(
            WellKnownType.System_Collections_Generic_Dictionary_2
        );
        var attributeValueType = context.WellKnownTypes.Get(
            WellKnownType.Amazon_DynamoDBv2_Model_AttributeValue
        );
        var stringType = context.WellKnownTypes.Get(SpecialType.System_String);

        if (type is not INamedTypeSymbol { IsGenericType: true } namedType)
            return false;

        if (!SymbolEqualityComparer.Default.Equals(namedType.ConstructedFrom, dictionaryType))
            return false;

        var typeArguments = namedType.TypeArguments;
        return typeArguments.Length == 2
            && SymbolEqualityComparer.Default.Equals(typeArguments[0], stringType)
            && SymbolEqualityComparer.Default.Equals(typeArguments[1], attributeValueType);
    }

    private static string? GetMethodSignature(IMethodSymbol? method)
    {
        if (method is null)
            return null;

        return method.ToDisplayString(MethodFormat);
    }
}
