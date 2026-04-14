using LayeredCraft.DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using WellKnownType =
    LayeredCraft.DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace LayeredCraft.DynamoMapper.Generator.Models;

internal sealed record MapperClassInfo(
    string Name,
    string Namespace,
    string ClassSignature,
    string? ToItemSignature,
    string? FromItemSignature,
    string? FromItemParameterName,
    string? ToItemParameterName,
    LocationInfo? Location,
    EquatableArray<HelperMethodInfo> HelperMethods,
    bool HasBeforeToItem,
    bool HasAfterToItem,
    bool HasBeforeFromItem,
    bool HasAfterFromItem
);

internal static class MapperClassInfoExtensions
{
    private const string ToMethodPrefix = "To";
    private const string FromMethodPrefix = "From";

    extension(MapperClassInfo)
    {
        internal static
            DiagnosticResult<( MapperClassInfo MapperClass, ITypeSymbol ModelType,
                EquatableArray<DiagnosticInfo> Diagnostics )> CreateAndResolveModelType(
                INamedTypeSymbol classSymbol, GeneratorContext context
            )
        {
            context.ThrowIfCancellationRequested();

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

            var methods = classSymbol.GetMembers().OfType<IMethodSymbol>().ToArray();

            var toItemMethod = methods.FirstOrDefault(m => IsToMethod(m, context));
            var fromItemMethod = methods.FirstOrDefault(m => IsFromMethod(m, context));

            // If there's an error in POCO type matching, propagate it
            return EnsurePocoTypesMatch(toItemMethod, fromItemMethod, classSymbol)
                .Bind(
                    modelType =>
                    {
                        var classSignature = GetClassSignature(classSymbol);
                        var toItemSignature = toItemMethod?.Map(GetMethodSignature);
                        var fromItemSignature = fromItemMethod?.Map(GetMethodSignature);
                        var namespaceStatement =
                            classSymbol.ContainingNamespace is { IsGlobalNamespace: false } ns
                                ? $"namespace {ns.ToDisplayString()};"
                                : string.Empty;

                        toItemMethod?.Parameters.FirstOrDefault()
                            ?.Name.Tap(name => context.MapperOptions.ToMethodParameterName = name);
                        fromItemMethod?.Parameters.FirstOrDefault()
                            ?.Name.Tap(
                                name => context.MapperOptions.FromMethodParameterName = name
                            );

                        var fromItemParameterName =
                            fromItemMethod?.Parameters.FirstOrDefault()?.Name;
                        var toItemParameterName = toItemMethod?.Parameters.FirstOrDefault()?.Name;

                        var hookAnalysis = AnalyzeHooks(methods, modelType, context);

                        var hasBeforeToItem =
                            toItemMethod is not null && hookAnalysis.HasBeforeToItem;
                        var hasAfterToItem =
                            toItemMethod is not null && hookAnalysis.HasAfterToItem;
                        var hasBeforeFromItem =
                            fromItemMethod is not null && hookAnalysis.HasBeforeFromItem;
                        var hasAfterFromItem =
                            fromItemMethod is not null && hookAnalysis.HasAfterFromItem;

                        if ((hasBeforeToItem || hasAfterToItem) && toItemParameterName is null)
                            return DiagnosticResult<( MapperClassInfo MapperClass, ITypeSymbol
                                ModelType, EquatableArray<DiagnosticInfo> Diagnostics )>.Failure(
                                DiagnosticDescriptors.InvalidHookSignature,
                                classSymbol.CreateLocationInfo(),
                                "ToItem",
                                "ToItem"
                            );

                        return
                            DiagnosticResult<( MapperClassInfo MapperClass, ITypeSymbol ModelType,
                                EquatableArray<DiagnosticInfo> Diagnostics )>.Success(
                                (new MapperClassInfo(classSymbol.Name, namespaceStatement, classSignature, toItemSignature, fromItemSignature, fromItemParameterName, toItemParameterName, context.TargetNode.CreateLocationInfo(), new EquatableArray<HelperMethodInfo>(), hasBeforeToItem, hasAfterToItem, hasBeforeFromItem, hasAfterFromItem),
                                    modelType, hookAnalysis.Diagnostics)
                            );
                    }
                );
        }
    }

    private static DiagnosticResult<ITypeSymbol> EnsurePocoTypesMatch(
        IMethodSymbol? toItemMethod, IMethodSymbol? fromItemMethod,
        INamedTypeSymbol mapperClassSymbol
    )
    {
        if (toItemMethod is null && fromItemMethod is null)
            return DiagnosticResult<ITypeSymbol>.Failure(
                DiagnosticDescriptors.NoMapperMethodsFound,
                mapperClassSymbol.CreateLocationInfo(),
                mapperClassSymbol.Name
            );

        var toItemPocoType = toItemMethod?.Parameters[0].Type;
        var fromItemPocoType = fromItemMethod?.ReturnType;

        if (toItemPocoType is not null && fromItemPocoType is not null &&
            !SymbolEqualityComparer.Default.Equals(toItemPocoType, fromItemPocoType))
            return DiagnosticResult<ITypeSymbol>.Failure(
                DiagnosticDescriptors.MismatchedPocoTypes,
                toItemMethod?.CreateLocationInfo(),
                toItemMethod?.Name,
                fromItemMethod?.Name,
                toItemPocoType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                fromItemPocoType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

        return DiagnosticResult<ITypeSymbol>.Success(toItemPocoType ?? fromItemPocoType!);
    }

    /// <summary>
    ///     To method must be:
    ///     <list type="bullet">
    ///         <item>partial</item> <item>not implemented</item>
    ///         <item>one parameter</item> <item>return an Attribute value dictionary</item>
    ///     </list>
    /// </summary>
    private static bool IsToMethod(IMethodSymbol method, GeneratorContext context) =>
        method.Name.StartsWith(ToMethodPrefix) && method is
        {
            IsPartialDefinition: true, PartialImplementationPart: null, Parameters.Length: 1,
        } && IsAttributeValueDictionary(method.ReturnType, context);

    /// <summary>
    ///     From method must be:
    ///     <list type="bullet">
    ///         <item>partial</item> <item>not implemented</item>
    ///         <item>one parameter</item> <item>parameter is an Attribute value dictionary</item>
    ///     </list>
    /// </summary>
    private static bool IsFromMethod(IMethodSymbol method, GeneratorContext context) =>
        method.Name.StartsWith(FromMethodPrefix) && method is
        {
            IsPartialDefinition: true, PartialImplementationPart: null, Parameters.Length: 1,
        } && IsAttributeValueDictionary(method.Parameters[0].Type, context);

    private sealed record HookAnalysisResult(
        bool HasBeforeToItem,
        bool HasAfterToItem,
        bool HasBeforeFromItem,
        bool HasAfterFromItem,
        EquatableArray<DiagnosticInfo> Diagnostics
    );

    private static HookAnalysisResult AnalyzeHooks(
        IMethodSymbol[] methods, ITypeSymbol modelType, GeneratorContext context
    )
    {
        var diagnostics = new List<DiagnosticInfo>();

        var hasBeforeToItem =
            IsHookPresent(methods, "BeforeToItem", modelType, context, diagnostics);
        var hasAfterToItem = IsHookPresent(methods, "AfterToItem", modelType, context, diagnostics);
        var hasBeforeFromItem =
            IsHookPresent(methods, "BeforeFromItem", modelType, context, diagnostics);
        var hasAfterFromItem =
            IsHookPresent(methods, "AfterFromItem", modelType, context, diagnostics);

        return new HookAnalysisResult(
            hasBeforeToItem,
            hasAfterToItem,
            hasBeforeFromItem,
            hasAfterFromItem,
            diagnostics.ToEquatableArray()
        );
    }

    private static bool IsAttributeValueDictionary(ITypeSymbol type, GeneratorContext context) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType && context.WellKnownTypes.IsType(
            namedType.ConstructedFrom,
            WellKnownType.System_Collections_Generic_Dictionary_2
        ) && namedType.TypeArguments.Length == 2 &&
        context.WellKnownTypes.IsType(namedType.TypeArguments[0], WellKnownType.System_String) &&
        context.WellKnownTypes.IsType(
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
        var extensionMethod = method.IsExtensionMethod ? "this " : string.Empty;

        return
            $"{accessibility} {modifiers}partial {returnType} {method.Name}({extensionMethod}{parameterType} {parameterName})";
    }

    private static bool IsHookPresent(
        IEnumerable<IMethodSymbol> methods, string name, ITypeSymbol modelType,
        GeneratorContext context, List<DiagnosticInfo> diagnostics
    )
    {
        var matchingMethods = methods.Where(m => m.Name == name).ToArray();
        var hasValidHook = false;

        foreach (var method in matchingMethods)
        {
            if (!method.IsStatic)
            {
                diagnostics.Add(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.HookNotStatic,
                        method.CreateLocationInfo(),
                        method.Name
                    )
                );
                continue;
            }

            if (!method.ReturnsVoid)
            {
                diagnostics.Add(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.InvalidHookSignature,
                        method.CreateLocationInfo(),
                        method.Name,
                        name
                    )
                );
                continue;
            }

            if (!HasExpectedParameterCount(method, name) || !HasExpectedRefKinds(method, name))
            {
                diagnostics.Add(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.InvalidHookSignature,
                        method.CreateLocationInfo(),
                        method.Name,
                        name
                    )
                );
                continue;
            }

            if (!HasExpectedParameterTypes(method, name, modelType, context))
            {
                diagnostics.Add(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.HookParameterTypeMismatch,
                        method.CreateLocationInfo(),
                        method.Name,
                        modelType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    )
                );
                continue;
            }

            hasValidHook = true;
        }

        return hasValidHook;
    }

    private static bool HasExpectedParameterCount(IMethodSymbol method, string hookName) =>
        method.Parameters.Length == hookName switch
        {
            "BeforeToItem" => 2,
            "AfterToItem" => 2,
            "BeforeFromItem" => 1,
            "AfterFromItem" => 2,
            _ => -1,
        };

    private static bool HasExpectedRefKinds(IMethodSymbol method, string hookName)
    {
        if (method.Parameters.Length == 0)
            return false;

        return hookName switch
        {
            "BeforeToItem" => method.Parameters[0].RefKind == RefKind.None &&
                method.Parameters[1].RefKind == RefKind.None,
            "AfterToItem" => method.Parameters[0].RefKind == RefKind.None &&
                method.Parameters[1].RefKind == RefKind.None,
            "BeforeFromItem" => method.Parameters[0].RefKind == RefKind.None,
            "AfterFromItem" => method.Parameters[0].RefKind == RefKind.None &&
                method.Parameters[1].RefKind == RefKind.Ref,
            _ => false,
        };
    }

    private static bool HasExpectedParameterTypes(
        IMethodSymbol method, string hookName, ITypeSymbol modelType, GeneratorContext context
    ) => hookName switch
    {
        "BeforeToItem" => IsModelType(method.Parameters[0].Type, modelType) &&
            IsAttributeValueDictionary(method.Parameters[1].Type, context),
        "AfterToItem" => IsModelType(method.Parameters[0].Type, modelType) &&
            IsAttributeValueDictionary(method.Parameters[1].Type, context),
        "BeforeFromItem" => IsAttributeValueDictionary(method.Parameters[0].Type, context),
        "AfterFromItem" => IsAttributeValueDictionary(method.Parameters[0].Type, context) &&
            IsModelType(method.Parameters[1].Type, modelType),
        _ => false,
    };

    private static bool IsModelType(ITypeSymbol hookType, ITypeSymbol modelType) =>
        SymbolEqualityComparer.Default.Equals(hookType, modelType);
}
