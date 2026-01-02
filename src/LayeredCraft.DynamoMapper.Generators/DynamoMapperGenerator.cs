using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Emitters;
using DynamoMapper.Runtime;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

[Generator]
public class DynamoMapperGenerator : IIncrementalGenerator
{
    private static readonly string DynamoMapperAttribute = typeof(DynamoMapperAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperInfos = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                DynamoMapperAttribute,
                MapperSyntaxProvider.Predicate,
                MapperSyntaxProvider.Transformer
            )
            .WithTrackingName(TrackingName.MapperSyntaxProvider_Extract)
            .WhereNotNull()
            .WithTrackingName(TrackingName.MapperSyntaxProvider_FilterNotNull);

        context.RegisterSourceOutput(
            mapperInfos,
            (ctx, info) =>
            {
                // Report any diagnostics collected during analysis
                info.Diagnostics.ForEach(diagnosticInfo => diagnosticInfo.ReportDiagnostic(ctx));

                // Skip code generation if there were errors
                if (
                    info.Diagnostics.Any(diagnosticInfo =>
                        diagnosticInfo.DiagnosticDescriptor.DefaultSeverity
                        == DiagnosticSeverity.Error
                    )
                )
                    return;

                MapperEmitter.Generate(ctx, info);
            }
        );
    }
}
