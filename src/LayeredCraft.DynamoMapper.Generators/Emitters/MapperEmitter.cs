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
        var toAssignments = mapperInfo
            .ModelClass!.Properties.Where(p => !string.IsNullOrEmpty(p.ToAssignments))
            .Select(p => p.ToAssignments)
            .ToArray();

        var fromAssignments = mapperInfo
            .ModelClass!.Properties.Where(p => !string.IsNullOrEmpty(p.FromAssignment))
            .Select(p => p.FromAssignment)
            .ToArray();

        var model = new
        {
            GeneratedCodeAttribute,
            mapperInfo.MapperClass,
            ModelClass = mapperInfo.ModelClass!,
            ToAssignments = toAssignments,
            FromAssignments = fromAssignments,
            MapperClassNamespace = mapperInfo.MapperClass?.Namespace,
            MapperClassSignature = mapperInfo.MapperClass?.ClassSignature,
            DictionaryCapacity = toAssignments.Length,
        };

        var outputCode = TemplateHelper.Render("Templates.Mapper.scriban", model);

        context.AddSource($"{mapperInfo.MapperClass?.Name}.g.cs", outputCode);
    }
}
