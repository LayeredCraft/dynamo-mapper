using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Emitters;
using DynamoMapper.Generator.Helpers;
using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

[Generator]
public class DynamoMapperGenerator : IIncrementalGenerator
{
    public static readonly string DynamoMapperAttribute =
        "DynamoMapper.Runtime.DynamoMapperAttribute";

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
                try
                {
                    // Report any diagnostics collected during analysis
                    info.Diagnostics.ForEach(diagnosticInfo =>
                        diagnosticInfo.ReportDiagnostic(ctx)
                    );

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
                catch (OperationCanceledException)
                {
                    // Cancellation is expected, don't report
                    throw;
                }
                catch (Exception ex)
                {
                    // Report emission error as diagnostic
                    ctx.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InternalGeneratorError,
                            info.MapperClass?.Location?.ToLocation(),
                            ExceptionHelper.FormatExceptionMessage(ex)
                        )
                    );
                }
            }
        );
    }
}
