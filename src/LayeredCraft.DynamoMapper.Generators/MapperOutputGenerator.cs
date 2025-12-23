using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal static class MapperOutputGenerator
{
    internal static string GeneratedCodeAttribute
    {
        get
        {
            if (field is null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var generatorName = assembly.GetName().Name;
                var generatorVersion = assembly.GetName().Version.ToString();

                field =
                    $"""[global::System.CodeDom.Compiler.GeneratedCode("{generatorName}", "{generatorVersion}")]""";
            }

            return field;
        }
    }

    private static int _counter;

    internal static void Generate(SourceProductionContext context, MapperInfo mapperInfo)
    {
        _counter++;

        var outputCode = $$"""
            namespace DynamoMapper.Generator;

            {{GeneratedCodeAttribute}}
            public static class Generated{{_counter}}{ }
            """;

        context.AddSource($"Generated{_counter}.g.cs", outputCode);
    }
}
