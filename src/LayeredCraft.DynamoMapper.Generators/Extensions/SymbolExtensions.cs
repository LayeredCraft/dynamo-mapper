using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis;

internal static class SymbolExtensions
{
    extension(IPropertySymbol property)
    {
        internal bool IsAssignableFrom(INamedTypeSymbol sourceType, Compilation compilation)
        {
            var conversion = compilation.ClassifyConversion(sourceType, property.Type);
            return conversion.IsImplicit;
        }

        internal bool IsAssignableTo(INamedTypeSymbol targetType, Compilation compilation)
        {
            var conversion = compilation.ClassifyConversion(property.Type, targetType);
            return conversion.IsImplicit;
        }

        internal bool IsAssignableFromOrTo(INamedTypeSymbol type, Compilation compilation) =>
            property.IsAssignableFrom(type, compilation)
            || property.IsAssignableTo(type, compilation);
    }
}
