using DynamoMapper.Generator.Diagnostics;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record MapperInfo(
    MapperClassInfo? MapperClass,
    ModelClassInfo? ModelClass,
    EquatableArray<DiagnosticInfo> Diagnostics
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
                return MapperInfo.CreateWithDiagnostics([mapperResult.Error!]);

            var (mapperClassInfo, modelTypeSymbol) = mapperResult.Value;

            var (modelClassInfo, diagnosticInfos) = ModelClassInfo.Create(modelTypeSymbol, context);

            return new MapperInfo(
                mapperClassInfo,
                modelClassInfo,
                diagnosticInfos.ToEquatableArray()
            );
        }

        /// <summary>
        ///     Creates a MapperInfo containing only error diagnostics. Used when an exception prevents
        ///     normal analysis.
        /// </summary>
        private static MapperInfo CreateWithDiagnostics(IEnumerable<DiagnosticInfo> diagnostics) =>
            new(null, null, diagnostics.ToEquatableArray());
    }
}
