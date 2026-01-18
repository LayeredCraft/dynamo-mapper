using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.ConstructorMapping.Models;

/// <summary>
///     Represents the semantic analysis of a constructor, including all its parameters.
/// </summary>
/// <param name="Constructor">The constructor method symbol.</param>
/// <param name="Parameters">The analyzed parameters of this constructor.</param>
/// <param name="HasMapperConstructorAttribute">True if this constructor has the [DynamoMapperConstructor] attribute.</param>
internal sealed record ConstructorAnalysis(
    IMethodSymbol Constructor,
    EquatableArray<ParameterAnalysis> Parameters,
    bool HasMapperConstructorAttribute
);
