using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal class GeneratorContext
{
    internal WellKnownTypes.WellKnownTypes WellKnownTypes { get; }
    internal CancellationToken CancellationToken { get; }
    internal SemanticModel SemanticModel { get; }
    internal GeneratorAttributeSyntaxContext GeneratorAttributeSyntaxContext { get; }

    internal GeneratorContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        GeneratorAttributeSyntaxContext = context;
        SemanticModel = GeneratorAttributeSyntaxContext.SemanticModel;
        CancellationToken = cancellationToken;
        WellKnownTypes = Generator.WellKnownTypes.WellKnownTypes.GetOrCreate(
            context.SemanticModel.Compilation
        );
    }
}

internal static class GeneratorContextExtensions
{
    extension(GeneratorContext context)
    {
        public void ThrowIfCancellationRequested() =>
            context.CancellationToken.ThrowIfCancellationRequested();
    }
}
