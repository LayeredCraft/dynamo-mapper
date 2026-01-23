using System.Collections.Immutable;
using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Context for nested object analysis, tracking ancestor types for cycle detection
///     and providing access to the mapper registry and field overrides.
/// </summary>
/// <param name="Context">The generator context.</param>
/// <param name="Registry">The registry of all discovered mappers.</param>
/// <param name="AncestorTypes">
///     Set of fully qualified type names currently being analyzed (for cycle detection).
/// </param>
/// <param name="CurrentPath">
///     The current dot-notation path being analyzed (e.g., "Address" or "Address.Country").
/// </param>
internal sealed record NestedAnalysisContext(
    GeneratorContext Context,
    MapperRegistry Registry,
    ImmutableHashSet<string> AncestorTypes,
    string CurrentPath = ""
)
{
    /// <summary>
    ///     Checks if analyzing the given type would create a cycle.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <returns>True if adding this type would create a cycle.</returns>
    internal bool WouldCreateCycle(ITypeSymbol typeSymbol)
    {
        var fullyQualifiedName = typeSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        return AncestorTypes.Contains(fullyQualifiedName);
    }

    /// <summary>
    ///     Creates a new context with the given type added to the ancestor chain.
    /// </summary>
    /// <param name="typeSymbol">The type to add as an ancestor.</param>
    /// <returns>A new context with the type added.</returns>
    internal NestedAnalysisContext WithAncestor(ITypeSymbol typeSymbol)
    {
        var fullyQualifiedName = typeSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        return this with { AncestorTypes = AncestorTypes.Add(fullyQualifiedName) };
    }

    /// <summary>
    ///     Creates a new context with the given property path appended.
    /// </summary>
    /// <param name="propertyName">The property name to append.</param>
    /// <returns>A new context with the updated path.</returns>
    internal NestedAnalysisContext WithPath(string propertyName)
    {
        var newPath = string.IsNullOrEmpty(CurrentPath)
            ? propertyName
            : $"{CurrentPath}.{propertyName}";
        return this with { CurrentPath = newPath };
    }

    /// <summary>
    ///     Checks if there are any dot-notation field overrides for the current path.
    /// </summary>
    /// <returns>True if any field options have overrides for properties under the current path.</returns>
    internal bool HasOverridesForCurrentPath()
    {
        if (string.IsNullOrEmpty(CurrentPath))
            return false;

        var pathPrefix = CurrentPath + ".";

        // Check if any field options have paths starting with the current path
        foreach (var key in Context.FieldOptions.Keys)
        {
            if (key.StartsWith(pathPrefix, StringComparison.Ordinal) || key == CurrentPath)
                return true;
        }

        // Check ignore options as well
        foreach (var key in Context.IgnoreOptions.Keys)
        {
            if (key.StartsWith(pathPrefix, StringComparison.Ordinal) || key == CurrentPath)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Gets field options for a nested property at the given path.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The field options if found, null otherwise.</returns>
    internal DynamoFieldOptions? GetFieldOptionsForProperty(string propertyName)
    {
        var fullPath = string.IsNullOrEmpty(CurrentPath)
            ? propertyName
            : $"{CurrentPath}.{propertyName}";

        return Context.FieldOptions.TryGetValue(fullPath, out var options) ? options : null;
    }

    /// <summary>
    ///     Checks if a nested property should be ignored.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The ignore options if the property should be ignored, null otherwise.</returns>
    internal DynamoIgnoreOptions? GetIgnoreOptionsForProperty(string propertyName)
    {
        var fullPath = string.IsNullOrEmpty(CurrentPath)
            ? propertyName
            : $"{CurrentPath}.{propertyName}";

        return Context.IgnoreOptions.TryGetValue(fullPath, out var options) ? options : null;
    }

    /// <summary>
    ///     Creates an initial context for nested analysis.
    /// </summary>
    /// <param name="context">The generator context.</param>
    /// <param name="registry">The mapper registry.</param>
    /// <returns>A new nested analysis context.</returns>
    internal static NestedAnalysisContext Create(GeneratorContext context, MapperRegistry registry) =>
        new(context, registry, ImmutableHashSet<string>.Empty);
}
