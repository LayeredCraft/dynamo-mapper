using System.Collections.Immutable;
using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Emitters;
using DynamoMapper.Generator.Models;
using DynamoMapper.Runtime;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

/// <summary>Source code generator for DynamoMapper.</summary>
[Generator]
public class DynamoMapperGenerator : IIncrementalGenerator
{
    private static readonly string DynamoMapperAttribute = typeof(DynamoMapperAttribute).FullName!;

    /// <summary>Initializes the DynamoMapper generator.</summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Extract lightweight mapper references for building the registry
        var mapperReferences = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                DynamoMapperAttribute,
                MapperSyntaxProvider.Predicate,
                MapperReferenceExtractor.Extract
            )
            .WithTrackingName(TrackingName.MapperReferenceExtractor_Extract)
            .Where(static x => x.HasValue)
            .Select(static (x, _) => x!.Value)
            .WithTrackingName(TrackingName.MapperReferenceExtractor_FilterNotNull);

        // Step 2: Collect all mapper references and build the registry
        var registryProvider = mapperReferences
            .Collect()
            .Select(static (refs, _) => BuildRegistry(refs))
            .WithTrackingName(TrackingName.MapperRegistry_Build);

        // Step 3: Get syntax contexts for full analysis
        var syntaxContexts = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                DynamoMapperAttribute,
                MapperSyntaxProvider.Predicate,
                static (ctx, _) => ctx
            )
            .WithTrackingName(TrackingName.MapperSyntaxContext_Extract);

        // Step 4: Combine each syntax context with the registry for full analysis
        var mapperInfos = syntaxContexts
            .Combine(registryProvider)
            .Select(
                static (pair, cancellationToken) =>
                    MapperSyntaxProvider.TransformerWithRegistry(
                        pair.Left,
                        pair.Right,
                        cancellationToken
                    )
            )
            .WithTrackingName(TrackingName.MapperSyntaxProvider_Extract)
            .WhereNotNull()
            .WithTrackingName(TrackingName.MapperSyntaxProvider_FilterNotNull);

        context.RegisterSourceOutput(
            mapperInfos,
            static (ctx, info) =>
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

    private static MapperRegistry BuildRegistry(
        ImmutableArray<(string ModelTypeFqn, MapperReference Reference)> references
    )
    {
        var builder = ImmutableDictionary.CreateBuilder<string, MapperReference>();

        foreach (var (modelTypeFqn, reference) in references)
        {
            // If multiple mappers map the same type, the last one wins
            // (this shouldn't happen in practice)
            builder[modelTypeFqn] = reference;
        }

        return new MapperRegistry(builder.ToImmutable());
    }
}
