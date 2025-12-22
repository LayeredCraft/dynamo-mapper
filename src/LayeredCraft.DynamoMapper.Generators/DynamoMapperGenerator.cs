using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

public readonly record struct GeneratorInfo;

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
            .Where(static m => m is not null)
            .WithTrackingName(TrackingName.MapperSyntaxProvider_FilterNotNull)
            .Select(static (m, _) => m!.Value)
            .WithTrackingName(TrackingName.MapperSyntaxProvider_GetValue);

        // context.RegisterSourceOutput();
    }
}
