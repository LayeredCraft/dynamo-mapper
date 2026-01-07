using DynamoMapper.Generator;
using Microsoft.CodeAnalysis.CSharp;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace Microsoft.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    extension(INamedTypeSymbol sourceType)
    {
        internal bool IsAssignableTo(INamedTypeSymbol targetType, GeneratorContext context) =>
            context.SemanticModel.Compilation.ClassifyConversion(sourceType, targetType).IsImplicit;

        internal bool IsAssignableFrom(INamedTypeSymbol targetType, GeneratorContext context) =>
            context.SemanticModel.Compilation.ClassifyConversion(targetType, sourceType).IsImplicit;

        internal bool IsAssignableFromOrTo(INamedTypeSymbol otherType, GeneratorContext context) =>
            sourceType.IsAssignableFrom(otherType, context)
            || sourceType.IsAssignableTo(otherType, context);

        internal bool IsAssignableTo(WellKnownType targetType, GeneratorContext context) =>
            sourceType.IsAssignableTo(context.WellKnownTypes.Get(targetType), context);

        internal bool IsAssignableFrom(WellKnownType targetType, GeneratorContext context) =>
            sourceType.IsAssignableFrom(context.WellKnownTypes.Get(targetType), context);

        internal bool IsAssignableFromOrTo(WellKnownType otherType, GeneratorContext context) =>
            sourceType.IsAssignableFromOrTo(context.WellKnownTypes.Get(otherType), context);
    }
}
