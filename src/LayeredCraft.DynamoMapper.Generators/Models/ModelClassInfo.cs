using DynamoMapper.Generator.ConstructorMapping;
using DynamoMapper.Generator.ConstructorMapping.Models;
using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record ModelClassInfo(
    string FullyQualifiedType,
    string VarName,
    EquatableArray<PropertyInfo> Properties,
    ConstructorInfo? Constructor
);

internal static class ModelClassInfoExtensions
{
    extension(ModelClassInfo)
    {
        internal static (ModelClassInfo?, DiagnosticInfo[]) Create(
            ITypeSymbol modelTypeSymbol,
            string? fromItemParameterName,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            var properties = GetModelProperties(modelTypeSymbol);

            var varName = GetModelVarName(modelTypeSymbol, fromItemParameterName, context);

            var constructorSelectionResult = SelectConstructorIfNeeded(
                modelTypeSymbol,
                properties,
                context
            );

            if (!constructorSelectionResult.IsSuccess)
                return (null, [constructorSelectionResult.Error!]);

            var selectedConstructor = constructorSelectionResult.Value;

            // Set the root model type for cycle detection in nested objects
            context.RootModelType = modelTypeSymbol as INamedTypeSymbol;

            // Validate dot-notation paths in field options
            var dotNotationDiagnostics = ValidateDotNotationPaths(modelTypeSymbol, context);
            if (dotNotationDiagnostics.Length > 0)
                return (null, dotNotationDiagnostics);

            var (propertyInfos, propertyInfosByIndex, propertyDiagnostics) = CreatePropertyInfos(
                properties,
                varName,
                selectedConstructor,
                context
            );

            var constructorInfo = selectedConstructor is null
                ? null
                : CreateConstructorInfo(selectedConstructor, properties, propertyInfosByIndex);

            var modelClassInfo = new ModelClassInfo(
                modelTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                varName,
                new EquatableArray<PropertyInfo>(propertyInfos),
                constructorInfo
            );

            return (modelClassInfo, propertyDiagnostics);
        }
    }

    private static IPropertySymbol[] GetModelProperties(ITypeSymbol modelTypeSymbol) =>
        modelTypeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => IsMappableProperty(p, modelTypeSymbol))
            .ToArray();

    private static bool IsMappableProperty(IPropertySymbol property, ITypeSymbol modelTypeSymbol) =>
        !property.IsStatic && !(modelTypeSymbol.IsRecord && property.Name == "EqualityContract");

    private static string GetModelVarName(
        ITypeSymbol modelTypeSymbol,
        string? fromItemParameterName,
        GeneratorContext context
    )
    {
        var varName = context.MapperOptions.KeyNamingConventionConverter(modelTypeSymbol.Name);
        return varName == fromItemParameterName ? varName + "1" : varName;
    }

    private static DiagnosticResult<ConstructorSelectionResult?> SelectConstructorIfNeeded(
        ITypeSymbol modelTypeSymbol,
        IPropertySymbol[] properties,
        GeneratorContext context
    ) =>
        context.HasFromItemMethod
            ? ConstructorSelector.Select(modelTypeSymbol, properties, context)
            : DiagnosticResult<ConstructorSelectionResult?>.Success(null);

    private static (
        PropertyInfo[] PropertyInfos,
        PropertyInfo?[] PropertyInfosByIndex,
        DiagnosticInfo[] Diagnostics
    ) CreatePropertyInfos(
        IPropertySymbol[] properties,
        string modelVarName,
        ConstructorSelectionResult? selectedConstructor,
        GeneratorContext context
    )
    {
        var initMethodsByPropertyName = selectedConstructor is null
            ? null
            : selectedConstructor.PropertyModes.ToDictionary(
                pm => pm.PropertyName,
                pm => pm.Method,
                StringComparer.Ordinal
            );

        var propertyDiagnosticsList = new List<DiagnosticInfo>();
        var propertyInfosList = new List<PropertyInfo>(properties.Length);

        var propertyInfosByIndex = new PropertyInfo?[properties.Length];

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            var initMethod = InitializationMethod.InitSyntax;
            if (
                initMethodsByPropertyName is not null
                && initMethodsByPropertyName.TryGetValue(property.Name, out var initMethod2)
            )
                initMethod = initMethod2;

            var propertyInfoResult = PropertyInfo.Create(
                property,
                modelVarName,
                i,
                initMethod,
                context
            );

            if (!propertyInfoResult.IsSuccess)
            {
                propertyDiagnosticsList.Add(propertyInfoResult.Error!);
                continue;
            }

            propertyInfosList.Add(propertyInfoResult.Value!);
            propertyInfosByIndex[i] = propertyInfoResult.Value!;
        }

        return (
            propertyInfosList.ToArray(),
            propertyInfosByIndex,
            propertyDiagnosticsList.ToArray()
        );
    }

    private static ConstructorInfo CreateConstructorInfo(
        ConstructorSelectionResult selectedConstructor,
        IPropertySymbol[] properties,
        PropertyInfo?[] propertyInfosByIndex
    )
    {
        var propertyIndexBySymbol = CreatePropertyIndexBySymbol(properties);

        var parameterInfosList = new List<ConstructorParameterInfo>(
            selectedConstructor.Constructor.Constructor.Parameters.Length
        );

        foreach (var paramAnalysis in selectedConstructor.Constructor.Parameters)
        {
            var matchingProperty = paramAnalysis.MatchedProperty;
            if (matchingProperty is null)
                continue;

            if (!propertyIndexBySymbol.TryGetValue(matchingProperty, out var propertyIndex))
                continue;

            var argument = propertyInfosByIndex[propertyIndex]?.FromConstructorArgument;

            if (argument is null)
                continue;

            parameterInfosList.Add(
                new ConstructorParameterInfo(paramAnalysis.MemberInfo.MemberName, argument)
            );
        }

        return new ConstructorInfo(
            new EquatableArray<ConstructorParameterInfo>(parameterInfosList.ToArray())
        );
    }

    private static Dictionary<IPropertySymbol, int> CreatePropertyIndexBySymbol(
        IPropertySymbol[] properties
    )
    {
        var propertyIndexBySymbol = new Dictionary<IPropertySymbol, int>(
            properties.Length,
            SymbolEqualityComparer.Default
        );

        for (var i = 0; i < properties.Length; i++)
            propertyIndexBySymbol[properties[i]] = i;

        return propertyIndexBySymbol;
    }

    /// <summary>
    ///     Validates that all dot-notation paths in field options and ignore options
    ///     refer to valid property paths on the model type.
    /// </summary>
    private static DiagnosticInfo[] ValidateDotNotationPaths(
        ITypeSymbol modelTypeSymbol,
        GeneratorContext context
    )
    {
        var diagnostics = new List<DiagnosticInfo>();

        // Validate field options paths
        foreach (var path in context.FieldOptions.Keys)
        {
            if (!path.Contains('.'))
                continue;

            var diagnostic = ValidatePath(path, modelTypeSymbol, context);
            if (diagnostic != null)
                diagnostics.Add(diagnostic);
        }

        // Validate ignore options paths
        foreach (var path in context.IgnoreOptions.Keys)
        {
            if (!path.Contains('.'))
                continue;

            var diagnostic = ValidatePath(path, modelTypeSymbol, context);
            if (diagnostic != null)
                diagnostics.Add(diagnostic);
        }

        return diagnostics.ToArray();
    }

    /// <summary>
    ///     Validates a dot-notation path against the model type hierarchy.
    /// </summary>
    private static DiagnosticInfo? ValidatePath(
        string path,
        ITypeSymbol rootType,
        GeneratorContext context
    )
    {
        var segments = path.Split('.');
        var currentType = rootType;

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            // Find the property on the current type
            var property = currentType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name == segment);

            if (property == null)
            {
                return new DiagnosticInfo(
                    DiagnosticDescriptors.InvalidDotNotationPath,
                    context.TargetNode.GetLocation().CreateLocationInfo(),
                    path,
                    segment,
                    currentType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                );
            }

            // For intermediate segments, navigate to the property type
            if (i < segments.Length - 1)
            {
                // Unwrap nullable if needed
                var propertyType = property.Type;
                if (propertyType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType
                    && nullableType.TypeArguments.Length == 1)
                {
                    propertyType = nullableType.TypeArguments[0];
                }

                currentType = propertyType;
            }
        }

        return null;
    }
}
