using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal class GeneratorContext
{
    internal GeneratorContext(
        GeneratorAttributeSyntaxContext context,
        WellKnownTypes.WellKnownTypes wellKnownTypes,
        MapperOptions mapperOptions,
        Dictionary<string, DynamoFieldOptions> fieldOptions,
        Dictionary<string, DynamoIgnoreOptions> ignoreOptions,
        CancellationToken cancellationToken
    )
    {
        GeneratorAttributeSyntaxContext = context;
        SemanticModel = GeneratorAttributeSyntaxContext.SemanticModel;
        CancellationToken = cancellationToken;
        WellKnownTypes = wellKnownTypes;
        TargetNode = GeneratorAttributeSyntaxContext.TargetNode;
        MapperOptions = mapperOptions;
        FieldOptions = fieldOptions;
        IgnoreOptions = ignoreOptions;
    }

    internal CancellationToken CancellationToken { get; }
    internal GeneratorAttributeSyntaxContext GeneratorAttributeSyntaxContext { get; }
    internal SyntaxNode TargetNode { get; }
    internal SemanticModel SemanticModel { get; }
    internal WellKnownTypes.WellKnownTypes WellKnownTypes { get; }
    internal MapperOptions MapperOptions { get; }
    internal Dictionary<string, DynamoFieldOptions> FieldOptions { get; }
    internal Dictionary<string, DynamoIgnoreOptions> IgnoreOptions { get; }

    /// <summary>
    ///     Indicates whether a ToItem method is defined in the mapper class. Used to determine if
    ///     properties need to be validated for serialization.
    /// </summary>
    internal bool HasToItemMethod { get; set; }

    /// <summary>
    ///     Indicates whether a FromItem method is defined in the mapper class. Used to determine if
    ///     properties need to be validated for deserialization.
    /// </summary>
    internal bool HasFromItemMethod { get; set; }
}

internal static class GeneratorContextExtensions
{
    extension(GeneratorContext context)
    {
        public void ThrowIfCancellationRequested() =>
            context.CancellationToken.ThrowIfCancellationRequested();
    }
}
