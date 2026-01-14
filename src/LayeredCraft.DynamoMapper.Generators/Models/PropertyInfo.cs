using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record PropertyInfo(string? FromAssignment, string? ToAssignments);

internal static class PropertyInfoExtensions
{
    extension(PropertyInfo)
    {
        /// <summary>
        ///     Creates a PropertyInfo from a property symbol using a functional pipeline. Pipeline:
        ///     PropertySymbol → Analysis → Strategy → Spec → Code → PropertyInfo
        /// </summary>
        internal static DiagnosticResult<PropertyInfo> Create(
            IPropertySymbol propertySymbol,
            GeneratorContext context
        ) =>
            PropertyAnalyzer
                .Analyze(propertySymbol, context)
                .Bind(analysis =>
                    TypeMappingStrategyResolver
                        .Resolve(analysis, context)
                        .Map(strategy => (analysis, strategy))
                )
                .Map(tuple =>
                    (
                        tuple.analysis,
                        PropertyMappingSpecBuilder.Build(tuple.analysis, tuple.strategy, context)
                    )
                )
                .Map(result =>
                    PropertyMappingCodeRenderer.Render(result.Item2, result.analysis, context)
                );
    }
}
