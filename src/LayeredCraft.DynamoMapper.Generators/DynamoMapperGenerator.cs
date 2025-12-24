using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal readonly record struct MapperAndDiagnosticInfo(
    MapperInfo MapperInfo,
    DiagnosticInfo? DiagnosticInfo
);

[Generator]
public class DynamoMapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperInfos = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                AttributeNames.DynamoMapperAttribute,
                MapperSyntaxProvider.Predicate,
                MapperSyntaxProvider.Transformer
            )
            .WithTrackingName(TrackingName.MapperSyntaxProvider_Extract)
            .WhereNotNull()
            .WithTrackingName(TrackingName.MapperSyntaxProvider_FilterNotNull);

        var mapperAndDiagnosticInfo = mapperInfos
            .Select(DiagnosticsProvider.Build)
            .WithTrackingName(TrackingName.DiagnosticsProvider);

        // output diagnostics
        context.RegisterSourceOutput(
            mapperAndDiagnosticInfo,
            static (ctx, info) =>
            {
                if (info.DiagnosticInfo is not null)
                    info.DiagnosticInfo!.Value.ReportDiagnostic(ctx);
            }
        );

        // mappers to build
        var mappersToBuild = mapperAndDiagnosticInfo.Where(static m => m.DiagnosticInfo is null);

        context.RegisterSourceOutput(
            mappersToBuild,
            (ctx, info) =>
            {
                MapperOutputGenerator.Generate(ctx, info.MapperInfo);
            }
        );
    }
}
