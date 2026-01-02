using DynamoMapper.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.Models;

internal sealed record MapperClassInfo(
    string Name,
    string Namespace,
    string ClassSignature,
    string? ToItemSignature,
    string? FromItemSignature,
    LocationInfo? Location
);

internal static class MapperClassInfoExtensions
{
    private const string ToMethodPrefix = "To";
    private const string FromMethodPrefix = "From";

    extension(MapperClassInfo)
    {
        internal static DiagnosticResult<(MapperClassInfo, ITypeSymbol)> CreateAndResolveModelType(
            ClassDeclarationSyntax classDeclaration,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(
                classDeclaration,
                context.CancellationToken
            );
            if (classSymbol is null)
                return DiagnosticResult<(MapperClassInfo, ITypeSymbol)>.Failure(
                    DiagnosticDescriptors.InternalGeneratorError,
                    classDeclaration.CreateLocationInfo(),
                    "Failed to resolve class symbol"
                );

            /*
             * to determine what mappers to generate, we need to look for methods using these rules:
             * - Methods starting with `To` (e.g., ToItem, ToModel) -> map from POCO to
             * AttributeValue
             * - Methods starting with `From` (e.g., FromItem, FromModel) -> map from AttributeValue
             * to POCO
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
                m.Name.StartsWith(ToMethodPrefix, StringComparison.Ordinal)
            );
            var fromItemMethod = methodSymbols.FirstOrDefault(static m =>
                m.Name.StartsWith(FromMethodPrefix, StringComparison.Ordinal)
            );

            var modelTypeResult = EnsurePocoTypesMatch(toItemMethod, fromItemMethod, classSymbol);

            // If there's an error in POCO type matching, propagate it
            if (!modelTypeResult.IsSuccess)
                return DiagnosticResult<(MapperClassInfo, ITypeSymbol)>.Failure(
                    modelTypeResult.Error!
                );

            var modelTypeSymbol = modelTypeResult.Value!;
            var classSignature = GetClassSignature(classSymbol);
            var toItemSignature = GetMethodSignature(toItemMethod);
            var fromItemSignature = GetMethodSignature(fromItemMethod);
            var namespaceStatement = classSymbol.ContainingNamespace
                is { IsGlobalNamespace: false } ns
                ? $"namespace {ns.ToDisplayString()};"
                : string.Empty;

            var mapperClassInfo = new MapperClassInfo(
                classSymbol.Name,
                namespaceStatement,
                classSignature,
                toItemSignature,
                fromItemSignature,
                classDeclaration.CreateLocationInfo()
            );

            return DiagnosticResult<(MapperClassInfo, ITypeSymbol)>.Success(
                (mapperClassInfo, modelTypeSymbol)
            );
        }
    }

    private static DiagnosticResult<ITypeSymbol> EnsurePocoTypesMatch(
        IMethodSymbol? toItemMethod,
        IMethodSymbol? fromItemMethod,
        INamedTypeSymbol mapperClassSymbol
    )
    {
        if (toItemMethod is null && fromItemMethod is null)
        {
            return DiagnosticResult<ITypeSymbol>.Failure(
                DiagnosticDescriptors.NoMapperMethodsFound,
                mapperClassSymbol.CreateLocationInfo(),
                mapperClassSymbol.Name
            );
        }

        var toItemPocoType = toItemMethod?.Parameters[0].Type;
        var fromItemPocoType = fromItemMethod?.ReturnType;

        if (
            toItemPocoType is not null
            && fromItemPocoType is not null
            && !SymbolEqualityComparer.Default.Equals(toItemPocoType, fromItemPocoType)
        )
        {
            return DiagnosticResult<ITypeSymbol>.Failure(
                DiagnosticDescriptors.MismatchedPocoTypes,
                toItemMethod?.CreateLocationInfo(),
                toItemMethod?.Name,
                fromItemMethod?.Name,
                toItemPocoType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                fromItemPocoType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
        }

        return DiagnosticResult<ITypeSymbol>.Success(toItemPocoType ?? fromItemPocoType!);
    }

    private static bool HasSupportedSignature(IMethodSymbol method, GeneratorContext context)
    {
        context.ThrowIfCancellationRequested();

        if (method.Parameters.Length != 1)
            return false;

        if (method.Name.StartsWith(ToMethodPrefix, StringComparison.Ordinal))
            return IsAttributeValueDictionary(method.ReturnType, context);

        if (method.Name.StartsWith(FromMethodPrefix, StringComparison.Ordinal))
            return !method.ReturnsVoid
                && IsAttributeValueDictionary(method.Parameters[0].Type, context);

        return false;
    }

    private static bool IsAttributeValueDictionary(ITypeSymbol type, GeneratorContext context)
    {
        context.ThrowIfCancellationRequested();

        if (type is not INamedTypeSymbol { IsGenericType: true } namedType)
            return false;

        if (
            !context.WellKnownTypes.IsType(
                namedType.ConstructedFrom,
                WellKnownType.System_Collections_Generic_Dictionary_2
            )
        )
            return false;

        var typeArguments = namedType.TypeArguments;
        return typeArguments.Length == 2
            && context.WellKnownTypes.IsType(typeArguments[0], WellKnownType.System_String)
            && context.WellKnownTypes.IsType(
                typeArguments[1],
                WellKnownType.Amazon_DynamoDBv2_Model_AttributeValue
            );
    }

    private static string GetClassSignature(INamedTypeSymbol classSymbol)
    {
        var accessibility = classSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        var modifiers = classSymbol.IsStatic ? "static " : string.Empty;

        return $"{accessibility} {modifiers}partial class {classSymbol.Name}";
    }

    private static string? GetMethodSignature(IMethodSymbol? method)
    {
        if (method is null)
            return null;

        // Build signature manually with hardcoded parameter name
        var parameter = method.Parameters[0];
        var parameterName = method.Name.StartsWith(ToMethodPrefix, StringComparison.Ordinal)
            ? "source"
            : "item";

        var returnType = method.ReturnType.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        var parameterType = parameter.Type.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        var accessibility = method.DeclaredAccessibility.ToString().ToLowerInvariant();
        var modifiers = method.IsStatic ? "static " : string.Empty;

        return $"{accessibility} {modifiers}partial {returnType} {method.Name}({parameterType} {parameterName})";
    }
}
