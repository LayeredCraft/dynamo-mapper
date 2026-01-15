using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record PropertyInfo(string? ToModelAssignment, string? FromModelAssignments);

internal static class PropertyInfoExtensions
{
    extension(PropertyInfo)
    {
        /// <summary>
        ///     Creates a PropertyInfo from a property symbol using a functional pipeline. Pipeline:
        ///     PropertySymbol → Analysis → Strategy → Spec → Code → PropertyInfo
        ///     Short-circuits when strategy is null (property won't be used in any method).
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
                .Bind<PropertyInfo>(tuple =>
                    // Short-circuit if property won't be used AND has no custom methods
                    tuple.strategy
                        is null
                    && tuple.analysis.FieldOptions?.ToMethod is null
                    && tuple.analysis.FieldOptions?.FromMethod is null
                        ? new PropertyInfo(null, null)
                        : PropertyMappingCodeRenderer.Render(
                            PropertyMappingSpecBuilder.Build(
                                tuple.analysis,
                                tuple.strategy,
                                context
                            ),
                            tuple.analysis,
                            context
                        )
                );
    }
}
