using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator;

internal static class MapperEmitter
{
    private static string GeneratedCodeAttribute
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

    internal static void Generate(SourceProductionContext context, MapperInfo mapperInfo)
    {
        var model = new
        {
            GeneratedCodeAttribute,
            mapperInfo.MapperClass,
            ModelClass = mapperInfo.ModelClass!.Value,
            MapperClassNamespace = mapperInfo.MapperClass.Namespace,
            MapperClassSignature = mapperInfo.MapperClass.ClassSignature,
            DictionaryCapacity = mapperInfo.ModelClass!.Value.ToAttributeAssignments.Count,
        };

        var outputCode = TemplateHelper.Render("Templates.Mapper.scriban", model);

        context.AddSource($"{mapperInfo.MapperClass.Name}.g.cs", outputCode);
    }
}
