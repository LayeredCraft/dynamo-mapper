using DynamoMapper.Generator.Models;
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
                info.Diagnostics.ForEach(diagnosticInfo => diagnosticInfo.ReportDiagnostic(ctx));

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
