using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record MapperInfo(
    MapperClassInfo? MapperClass,
    ModelClassInfo? ModelClass,
    EquatableArray<DiagnosticInfo> Diagnostics,
    GeneratorContext? Context,
    HelperMethodRegistry? HelperRegistry
);

internal static class MapperInfoExtensions
{
    extension(MapperInfo)
    {
        internal static MapperInfo Create(INamedTypeSymbol classSymbol, GeneratorContext context)
        {
            context.ThrowIfCancellationRequested();

            var mapperResult = MapperClassInfo.CreateAndResolveModelType(classSymbol, context);

            // If there's an error creating the mapper class info, return a MapperInfo with the
            // error
            if (!mapperResult.IsSuccess)
                return MapperInfo.CreateWithDiagnostics([mapperResult.Error!], context);

            var (mapperClassInfo, modelTypeSymbol) = mapperResult.Value;

            // Set method context flags so property validation knows which methods exist
            context.HasToItemMethod = mapperClassInfo.ToItemSignature != null;
            context.HasFromItemMethod = mapperClassInfo.FromItemSignature != null;

            // Create registry to track helper methods for nested objects
            var helperRegistry = new HelperMethodRegistry();

            var (modelClassInfo, diagnosticInfos, helperMethods) =
                ModelClassInfo.Create(
                    modelTypeSymbol,
                    mapperClassInfo.FromItemParameterName,
                    context,
                    helperRegistry
                );

            // Add helper methods to mapper class info
            var updatedMapperClassInfo =
                mapperClassInfo with
                {
                    HelperMethods = new EquatableArray<HelperMethodInfo>(helperMethods),
                };

            return new MapperInfo(
                updatedMapperClassInfo,
                modelClassInfo,
                diagnosticInfos.ToEquatableArray(),
                context,
                helperRegistry
            );
        }

        /// <summary>
        ///     Creates a MapperInfo containing only error diagnostics. Used when an exception prevents
        ///     normal analysis.
        /// </summary>
        private static MapperInfo CreateWithDiagnostics(
            IEnumerable<DiagnosticInfo> diagnostics, GeneratorContext? context = null,
            HelperMethodRegistry? helperRegistry = null
        ) => new(null, null, diagnostics.ToEquatableArray(), context, helperRegistry);
    }
}
