using DynamoMapper.Generator.ConstructorMapping.Models;
using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.ConstructorMapping;

/// <summary>
///     Centralized logic for selecting which constructor to use for deserialization.
///     Implements the constructor selection priority rules.
/// </summary>
internal static class ConstructorSelector
{
    /// <summary>
    ///     Selects constructor and determines how each property should be initialized.
    ///     Returns null if parameterless constructor + property initialization should be used.
    /// </summary>
    /// <param name="modelType">The model type symbol.</param>
    /// <param name="properties">All properties on the model type.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>
    ///     Success with null = use property initialization only.
    ///     Success with ConstructorSelectionResult = use specified constructor.
    ///     Failure with diagnostic = constructor selection error.
    /// </returns>
    internal static DiagnosticResult<ConstructorSelectionResult?> Select(
        ITypeSymbol modelType,
        IPropertySymbol[] properties,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        // Get all instance constructors
        var constructors = modelType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && !m.IsStatic)
            .ToArray();

        if (constructors.Length == 0)
            return DiagnosticResult<ConstructorSelectionResult?>.Success(null);

        // PRIORITY 1: Check for attributed constructors
        var attributedResult = FindAttributedConstructor(constructors, context);
        if (!attributedResult.IsSuccess)
            return DiagnosticResult<ConstructorSelectionResult?>.Failure(attributedResult.Error!);

        if (attributedResult.Value is not null)
        {
            return AnalyzeConstructorSelection(attributedResult.Value, properties, true, context);
        }

        // PRIORITY 2: Check if parameterless constructor exists
        var hasParameterlessConstructor = constructors.Any(c => c.Parameters.Length == 0);

        if (!hasParameterlessConstructor)
        {
            // NO parameterless constructor → MUST use a non-parameterless constructor
            var selectedConstructor = SelectConstructorWithMostParameters(constructors);
            return AnalyzeConstructorSelection(selectedConstructor, properties, false, context);
        }

        // PRIORITY 3: Parameterless constructor exists - check deserialization accessibility
        // Ignore computed/read-only properties that can't be populated from the item anyway.
        if (AllPropertiesAccessible(properties, constructors))
        {
            // Use property initialization (parameterless + object initializer)
            return DiagnosticResult<ConstructorSelectionResult?>.Success(null);
        }

        // PRIORITY 4: Has read-only properties - use constructor with most parameters
        var selectedConstructor2 = SelectConstructorWithMostParameters(constructors);
        return AnalyzeConstructorSelection(selectedConstructor2, properties, false, context);
    }

    /// <summary>
    ///     Finds the constructor with [DynamoMapperConstructor] attribute.
    ///     Returns error DM0103 if multiple attributed constructors found.
    /// </summary>
    private static DiagnosticResult<IMethodSymbol?> FindAttributedConstructor(
        IMethodSymbol[] constructors,
        GeneratorContext context
    )
    {
        var attributeType = context.WellKnownTypes.Get(
            WellKnownType.DynamoMapper_Runtime_DynamoMapperConstructorAttribute
        );

        if (attributeType is null)
            return DiagnosticResult<IMethodSymbol?>.Success(null);

        var attributedConstructors = constructors
            .Where(c =>
                c.GetAttributes()
                    .Any(attr =>
                        SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType)
                    )
            )
            .ToArray();

        if (attributedConstructors.Length == 0)
            return DiagnosticResult<IMethodSymbol?>.Success(null);

        if (attributedConstructors.Length > 1)
        {
            // DM0103: Multiple [DynamoMapperConstructor] attributes found
            return DiagnosticResult<IMethodSymbol?>.Failure(
                DiagnosticDescriptors.MultipleConstructorsWithAttribute,
                attributedConstructors[1].Locations.FirstOrDefault()?.CreateLocationInfo(),
                attributedConstructors[0].ContainingType.ToDisplayString()
            );
        }

        return DiagnosticResult<IMethodSymbol?>.Success(attributedConstructors[0]);
    }

    /// <summary>
    ///     Checks if the model can be deserialized using a parameterless constructor plus property
    ///     assignments (object initializer + post-construction assignments).
    /// </summary>
    /// <remarks>
    ///     Read-only properties should only force constructor-based deserialization if there is a
    ///     constructor parameter that can populate them. Computed properties (e.g. expression-bodied
    ///     getters) are ignored for deserialization and should not affect constructor selection.
    /// </remarks>
    private static bool AllPropertiesAccessible(
        IPropertySymbol[] properties,
        IMethodSymbol[] constructors
    )
    {
        foreach (var property in properties)
        {
            // Settable (including init-only) properties can be initialized via object initializer
            // or assignment.
            if (property.SetMethod is not null)
                continue;

            // Read-only property: only matters for deserialization if a constructor parameter can
            // populate it.
            var hasMatchingConstructorParameter = constructors.Any(c =>
                c.Parameters.Any(p =>
                    string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)
                )
            );

            if (hasMatchingConstructorParameter)
                return false;
        }

        return true;
    }

    /// <summary>Selects the constructor with the most parameters.</summary>
    private static IMethodSymbol SelectConstructorWithMostParameters(
        IMethodSymbol[] constructors
    ) => constructors.OrderByDescending(c => c.Parameters.Length).First();

    /// <summary>Analyzes the selected constructor and determines how each property should be initialized.</summary>
    private static DiagnosticResult<ConstructorSelectionResult?> AnalyzeConstructorSelection(
        IMethodSymbol constructor,
        IPropertySymbol[] properties,
        bool isExplicitlyAttributed,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        // Analyze constructor parameters
        var parameterAnalyses = new List<ParameterAnalysis>();
        for (var i = 0; i < constructor.Parameters.Length; i++)
        {
            var param = constructor.Parameters[i];
            var memberInfoResult = MemberAnalyzer.AnalyzeParameter(param, context);

            if (!memberInfoResult.IsSuccess)
                return DiagnosticResult<ConstructorSelectionResult?>.Failure(
                    memberInfoResult.Error!
                );

            // Match parameter to property (case-insensitive)
            var matchedProperty = properties.FirstOrDefault(p =>
                string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase)
            );

            parameterAnalyses.Add(
                new ParameterAnalysis(memberInfoResult.Value!, i, matchedProperty)
            );
        }

        var constructorAnalysis = new ConstructorAnalysis(
            constructor,
            new EquatableArray<ParameterAnalysis>(parameterAnalyses.ToArray()),
            isExplicitlyAttributed
        );

        // Determine initialization method for each property
        var propertyModes = new List<PropertyInitializationMode>();
        foreach (var property in properties)
        {
            var initMethod = DetermineInitializationMethod(property, constructorAnalysis);
            propertyModes.Add(new PropertyInitializationMode(property.Name, initMethod));
        }

        return new ConstructorSelectionResult(
            constructorAnalysis,
            new EquatableArray<PropertyInitializationMode>(propertyModes.ToArray())
        );
    }

    /// <summary>Determines how a specific property should be initialized given the selected constructor.</summary>
    private static InitializationMethod DetermineInitializationMethod(
        IPropertySymbol property,
        ConstructorAnalysis constructor
    )
    {
        // Check if property matches a constructor parameter (case-insensitive)
        var hasMatchingParam = constructor.Parameters.Any(p =>
            p.MatchedProperty is not null
            && SymbolEqualityComparer.Default.Equals(p.MatchedProperty, property)
        );

        if (hasMatchingParam)
            return InitializationMethod.ConstructorParameter;

        // Property not in constructor - check if it's settable
        var isInitOnly = property.SetMethod?.IsInitOnly ?? false;
        var hasSetter = property.SetMethod is not null;

        if (!hasSetter)
        {
            // Read-only property not in constructor - this shouldn't happen in valid scenarios
            // but we'll treat it as constructor parameter (will likely cause a compile error)
            return InitializationMethod.ConstructorParameter;
        }

        // Property is settable
        return isInitOnly ? InitializationMethod.InitSyntax : InitializationMethod.PostConstruction;
    }
}
