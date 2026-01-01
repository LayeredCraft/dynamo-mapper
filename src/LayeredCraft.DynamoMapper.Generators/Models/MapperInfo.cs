using LayeredCraft.SourceGeneratorTools.Types;

namespace DynamoMapper.Generator.Models;

internal sealed record MapperInfo(
    MapperClassInfo MapperClass,
    ModelClassInfo? ModelClass,
    EquatableArray<DiagnosticInfo> Diagnostics
);
