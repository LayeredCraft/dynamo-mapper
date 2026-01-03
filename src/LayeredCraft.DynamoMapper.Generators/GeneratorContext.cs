using DynamoMapper.Runtime;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal class GeneratorContext
{
    internal GeneratorContext(
        GeneratorAttributeSyntaxContext context,
        WellKnownTypes.WellKnownTypes wellKnownTypes,
        MapperOptions mapperOptions,
        CancellationToken cancellationToken
    )
    {
        GeneratorAttributeSyntaxContext = context;
        SemanticModel = GeneratorAttributeSyntaxContext.SemanticModel;
        CancellationToken = cancellationToken;
        WellKnownTypes = wellKnownTypes;
        TargetNode = GeneratorAttributeSyntaxContext.TargetNode;
        MapperOptions = mapperOptions;
    }

    internal CancellationToken CancellationToken { get; }
    internal GeneratorAttributeSyntaxContext GeneratorAttributeSyntaxContext { get; }
    internal SyntaxNode TargetNode { get; }
    internal SemanticModel SemanticModel { get; }
    internal WellKnownTypes.WellKnownTypes WellKnownTypes { get; }
    internal MapperOptions MapperOptions { get; }

    internal Func<string, string> KeyNamingConventionConverter
    {
        get
        {
            field ??= MapperOptions.Convention switch
            {
                DynamoNamingConvention.Exact => s => s,
                DynamoNamingConvention.CamelCase => InflectorExtensions.Camelize,
                DynamoNamingConvention.PascalCase => InflectorExtensions.Pascalize,
                DynamoNamingConvention.SnakeCase => InflectorExtensions.Underscore,
                DynamoNamingConvention.KebabCase => InflectorExtensions.Kebaberize,
                _ => throw new InvalidOperationException(
                    $"Unknown {nameof(DynamoNamingConvention)} of {MapperOptions.Convention}"
                ),
            };

            return field;
        }
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
