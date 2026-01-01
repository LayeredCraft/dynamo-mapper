using LayeredCraft.SourceGeneratorTools.Types;

namespace DynamoMapper.Generator.Models;

internal readonly record struct MapperInfo(
    MapperClassInfo MapperClass,
    ModelClassInfo? ModelClass,
    EquatableArray<DiagnosticInfo> Diagnostics
);
