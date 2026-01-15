using System.Reflection;
using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Emitters;

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
        var fromModelAssignments = mapperInfo
            .ModelClass!.Properties.Where(p => !string.IsNullOrEmpty(p.FromModelAssignments))
            .Select(p => p.FromModelAssignments)
            .ToArray();

        var toModelAssignments = mapperInfo
            .ModelClass!.Properties.Where(p => !string.IsNullOrEmpty(p.ToModelAssignment))
            .Select(p => p.ToModelAssignment)
            .ToArray();

        var model = new
        {
            GeneratedCodeAttribute,
            mapperInfo.MapperClass,
            ModelClass = mapperInfo.ModelClass!,
            FromModelAssignments = fromModelAssignments,
            ToModelAssignments = toModelAssignments,
            MapperClassNamespace = mapperInfo.MapperClass?.Namespace,
            MapperClassSignature = mapperInfo.MapperClass?.ClassSignature,
            DictionaryCapacity = fromModelAssignments.Length,
        };

        var outputCode = TemplateHelper.Render("Templates.Mapper.scriban", model);

        context.AddSource($"{mapperInfo.MapperClass?.Name}.g.cs", outputCode);
    }
}
