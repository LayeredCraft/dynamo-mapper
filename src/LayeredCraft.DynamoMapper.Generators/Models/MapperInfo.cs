using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoMapper.Generator.Models;

internal sealed record MapperInfo(
    MapperClassInfo MapperClass,
    ModelClassInfo? ModelClass,
    EquatableArray<DiagnosticInfo> Diagnostics
);

internal static class MapperInfoExtensions
{
    extension(MapperInfo)
    {
        internal static MapperInfo Create(
            ClassDeclarationSyntax classDeclarationSyntax,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            var mapperResult = MapperClassInfo.CreateAndResolveModelType(
                classDeclarationSyntax,
                context
            );
            if (mapperResult is null)
                return null;

            var (mapperClassInfo, modelTypeSymbol) = mapperResult.Value;

            var (modelClassInfo, diagnosticInfos) = ModelClassInfo.Create(modelTypeSymbol, context);

            return new MapperInfo(
                mapperClassInfo,
                modelClassInfo,
                diagnosticInfos.ToEquatableArray()
            );
        }
    }
}
