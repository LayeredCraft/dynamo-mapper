namespace DynamoMapper.Generator;

internal readonly record struct MapperAndDiagnosticInfo(
    MapperInfo MapperInfo,
    DiagnosticInfo? DiagnosticInfo
);
