using DynamoMapper.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.Models;

internal sealed record MapperClassInfo(
    string Name,
    string Namespace,
    string ClassSignature,
    string? FromModelSignature,
    string? ToModelSignature,
    LocationInfo? Location
);

internal static class MapperClassInfoExtensions
{
    private const string ToMethodPrefix = "To";
    private const string FromMethodPrefix = "From";

    extension(MapperClassInfo)
    {
        internal static DiagnosticResult<(MapperClassInfo, ITypeSymbol)> CreateAndResolveModelType(
            INamedTypeSymbol classSymbol,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            /*
             * to determine what mappers to generate, we need to look for methods using these rules:
             * - Methods starting with `From` (e.g., FromModel, FromProduct) -> map from POCO/Model
             * to
             * AttributeValue (Model → DynamoDB)
             * - Methods starting with `To` (e.g., ToModel, ToProduct) -> map from AttributeValue
             * to POCO/Model (DynamoDB → Model)
             *
             * Rules:
             * - at least one is needed, but both are not required
             * - the POCO type on both mapper methods must match.
             */

            var methods = classSymbol.GetMembers().OfType<IMethodSymbol>().ToArray();

            var fromModelMethod = methods.FirstOrDefault(m => IsFromMethod(m, context));
            var toModelMethod = methods.FirstOrDefault(m => IsToMethod(m, context));

            // If there's an error in POCO type matching, propagate it
            return EnsurePocoTypesMatch(fromModelMethod, toModelMethod, classSymbol)
                .Bind(modelType =>
                {
                    var classSignature = GetClassSignature(classSymbol);
                    var fromModelSignature = fromModelMethod?.Map(GetMethodSignature);
                    var toModelSignature = toModelMethod?.Map(GetMethodSignature);
                    var namespaceStatement = classSymbol.ContainingNamespace
                        is { IsGlobalNamespace: false } ns
                        ? $"namespace {ns.ToDisplayString()};"
                        : string.Empty;

                    fromModelMethod
                        ?.Parameters.FirstOrDefault()
                        ?.Name.Tap(name => context.MapperOptions.FromModelParameterName = name);
                    toModelMethod
                        ?.Parameters.FirstOrDefault()
                        ?.Name.Tap(name => context.MapperOptions.ToModelParameterName = name);

                    return DiagnosticResult<(MapperClassInfo, ITypeSymbol)>.Success(
                        (
                            new MapperClassInfo(
                                classSymbol.Name,
                                namespaceStatement,
                                classSignature,
                                fromModelSignature,
                                toModelSignature,
                                context.TargetNode.CreateLocationInfo()
                            ),
                            modelType
                        )
                    );
                });
        }
    }

    private static DiagnosticResult<ITypeSymbol> EnsurePocoTypesMatch(
        IMethodSymbol? fromModelMethod,
        IMethodSymbol? toModelMethod,
        INamedTypeSymbol mapperClassSymbol
    )
    {
        if (fromModelMethod is null && toModelMethod is null)
            return DiagnosticResult<ITypeSymbol>.Failure(
                DiagnosticDescriptors.NoMapperMethodsFound,
                mapperClassSymbol.CreateLocationInfo(),
                mapperClassSymbol.Name
            );

        var fromModelPocoType = fromModelMethod?.Parameters[0].Type;
        var toModelPocoType = toModelMethod?.ReturnType;

        if (
            fromModelPocoType is not null
            && toModelPocoType is not null
            && !SymbolEqualityComparer.Default.Equals(fromModelPocoType, toModelPocoType)
        )
            return DiagnosticResult<ITypeSymbol>.Failure(
                DiagnosticDescriptors.MismatchedPocoTypes,
                fromModelMethod?.CreateLocationInfo(),
                fromModelMethod?.Name,
                toModelMethod?.Name,
                fromModelPocoType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                toModelPocoType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

        return DiagnosticResult<ITypeSymbol>.Success(fromModelPocoType ?? toModelPocoType!);
    }

    /// <summary>
    ///     To method (ToModel, ToProduct, etc.) maps DynamoDB to Model (DynamoDB → Model).
    ///     Must be:
    ///     <list type="bullet">
    ///         <item>partial</item> <item>not implemented</item>
    ///         <item>one parameter</item> <item>parameter is an Attribute value dictionary</item>
    ///     </list>
    /// </summary>
    private static bool IsToMethod(IMethodSymbol method, GeneratorContext context) =>
        method.Name.StartsWith(ToMethodPrefix)
        && method
            is { IsPartialDefinition: true, PartialImplementationPart: null, Parameters.Length: 1 }
        && IsAttributeValueDictionary(method.Parameters[0].Type, context);

    /// <summary>
    ///     From method (FromModel, FromProduct, etc.) maps Model to DynamoDB (Model → DynamoDB).
    ///     Must be:
    ///     <list type="bullet">
    ///         <item>partial</item> <item>not implemented</item>
    ///         <item>one parameter</item> <item>return an Attribute value dictionary</item>
    ///     </list>
    /// </summary>
    private static bool IsFromMethod(IMethodSymbol method, GeneratorContext context) =>
        method.Name.StartsWith(FromMethodPrefix)
        && method
            is { IsPartialDefinition: true, PartialImplementationPart: null, Parameters.Length: 1 }
        && IsAttributeValueDictionary(method.ReturnType, context);

    private static bool IsAttributeValueDictionary(ITypeSymbol type, GeneratorContext context) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType
        && context.WellKnownTypes.IsType(
            namedType.ConstructedFrom,
            WellKnownType.System_Collections_Generic_Dictionary_2
        )
        && namedType.TypeArguments.Length == 2
        && context.WellKnownTypes.IsType(namedType.TypeArguments[0], WellKnownType.System_String)
        && context.WellKnownTypes.IsType(
            namedType.TypeArguments[1],
            WellKnownType.Amazon_DynamoDBv2_Model_AttributeValue
        );

    private static string GetClassSignature(INamedTypeSymbol classSymbol)
    {
        var accessibility = classSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        var modifiers = classSymbol.IsStatic ? "static " : string.Empty;

        return $"{accessibility} {modifiers}partial class {classSymbol.Name}";
    }

    private static string GetMethodSignature(IMethodSymbol method)
    {
        var parameter = method.Parameters.FirstOrDefault();
        var parameterName = parameter?.Name;
        var returnType = method.ReturnType.QualifiedNullableName;
        var parameterType = parameter?.Type.QualifiedNullableName;
        var accessibility = method.DeclaredAccessibility.ToString().ToLowerInvariant();
        var modifiers = method.IsStatic ? "static " : string.Empty;

        return $"{accessibility} {modifiers}partial {returnType} {method.Name}({parameterType} {parameterName})";
    }
}
