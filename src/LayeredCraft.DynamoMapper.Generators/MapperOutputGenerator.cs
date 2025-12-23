using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal static class MapperOutputGenerator
{
    private static int _counter;

    internal static void Generate(SourceProductionContext context, MapperInfo mapperInfo)
    {
        _counter++;

        var outputCode = $$"""
            namespace DynamoMapper.Generator;

            public static class Generated{{_counter}}{ }
            """;

        context.AddSource($"Generated{_counter}.g.cs", outputCode);
    }
}
