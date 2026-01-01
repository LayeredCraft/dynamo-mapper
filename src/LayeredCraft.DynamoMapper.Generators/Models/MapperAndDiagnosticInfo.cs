namespace DynamoMapper.Generator.Models;

internal readonly record struct MapperAndDiagnosticInfo(
    MapperInfo MapperInfo,
    DiagnosticInfo? DiagnosticInfo
);
