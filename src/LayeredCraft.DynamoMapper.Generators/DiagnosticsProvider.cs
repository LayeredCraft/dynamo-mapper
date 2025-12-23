namespace DynamoMapper.Generator;

internal static class DiagnosticsProvider
{
    internal static MapperAndDiagnosticInfo Build(
        MapperInfo mapperInfo,
        CancellationToken cancellationToken
    ) => new(mapperInfo, null);
}
