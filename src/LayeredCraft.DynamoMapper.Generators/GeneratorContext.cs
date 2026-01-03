using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal class GeneratorContext
{
    internal GeneratorContext(
        GeneratorAttributeSyntaxContext context,
        WellKnownTypes.WellKnownTypes wellKnownTypes,
        CancellationToken cancellationToken
    )
    {
        GeneratorAttributeSyntaxContext = context;
        SemanticModel = GeneratorAttributeSyntaxContext.SemanticModel;
        CancellationToken = cancellationToken;
        WellKnownTypes = wellKnownTypes;
        TargetNode = GeneratorAttributeSyntaxContext.TargetNode;
    }

    internal CancellationToken CancellationToken { get; }
    internal GeneratorAttributeSyntaxContext GeneratorAttributeSyntaxContext { get; }
    internal SyntaxNode TargetNode { get; }
    internal SemanticModel SemanticModel { get; }
    internal WellKnownTypes.WellKnownTypes WellKnownTypes { get; }
    internal MapperOptions MapperOptions { get; set; } = new();
}

internal static class GeneratorContextExtensions
{
    extension(GeneratorContext context)
    {
        public void ThrowIfCancellationRequested() =>
            context.CancellationToken.ThrowIfCancellationRequested();

        internal void PopulateMapperOptions() { }
    }
}
