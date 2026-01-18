using LayeredCraft.SourceGeneratorTools.Types;

namespace DynamoMapper.Generator.ConstructorMapping.Models;

/// <summary>
///     Represents the result of constructor selection, including how each property should be
///     initialized.
/// </summary>
/// <param name="Constructor">The selected constructor analysis.</param>
/// <param name="PropertyModes">The initialization method for each property.</param>
internal sealed record ConstructorSelectionResult(
    ConstructorAnalysis Constructor,
    EquatableArray<PropertyInitializationMode> PropertyModes
);
