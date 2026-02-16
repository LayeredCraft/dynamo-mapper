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
        var toAssignments =
            mapperInfo.ModelClass!.Properties.Where(p => !string.IsNullOrEmpty(p.ToAssignments))
                .Select(p => p.ToAssignments)
                .ToArray();

        var fromAssignments =
            mapperInfo.ModelClass!.Properties.Where(p => !string.IsNullOrEmpty(p.FromAssignment))
                .Select(p => p.FromAssignment)
                .ToArray();

        var fromInitAssignments =
            mapperInfo.ModelClass!.Properties
                .Where(p => !string.IsNullOrEmpty(p.FromInitAssignment))
                .Select(p => p.FromInitAssignment)
                .ToArray();

        // Render helper methods for nested objects
        // We need to render iteratively because rendering a helper might register new helpers
        var helperMethods = Array.Empty<string>();
        if (mapperInfo.MapperClass is not null && mapperInfo.Context is not null &&
            mapperInfo.HelperRegistry is not null && mapperInfo.MapperClass.HelperMethods.Any())
        {
            var renderedHelpers = new HashSet<string>();
            var helperList = new List<string>();

            // Keep rendering until all helpers are processed
            while (true)
            {
                var allHelpers = mapperInfo.HelperRegistry.GetAllHelpers();
                var newHelpers =
                    allHelpers.Where(h => !renderedHelpers.Contains(h.MethodName)).ToArray();

                if (newHelpers.Length == 0)
                    break;

                foreach (var helper in newHelpers)
                {
                    var rendered =
                        helper.Direction == HelperMethodDirection.ToItem
                            ? HelperMethodEmitter.RenderToItemHelper(
                                helper,
                                mapperInfo.Context!,
                                mapperInfo.HelperRegistry!
                            )
                            : HelperMethodEmitter.RenderFromItemHelper(
                                helper,
                                mapperInfo.Context!,
                                mapperInfo.HelperRegistry!
                            );

                    helperList.Add(rendered);
                    renderedHelpers.Add(helper.MethodName);
                }
            }

            helperMethods = helperList.ToArray();
        }

        var model =
            new
            {
                GeneratedCodeAttribute,
                mapperInfo.MapperClass,
                ModelClass = mapperInfo.ModelClass!,
                ModelVarName = mapperInfo.ModelClass.VarName,
                ToAssignments = toAssignments,
                FromAssignments = fromAssignments,
                FromInitAssignments = fromInitAssignments,
                MapperClassNamespace = mapperInfo.MapperClass?.Namespace,
                MapperClassSignature = mapperInfo.MapperClass?.ClassSignature,
                DictionaryCapacity = toAssignments.Length,
                HelperMethods = helperMethods,
            };

        var outputCode = TemplateHelper.Render("Templates.Mapper.scriban", model);

        context.AddSource($"{mapperInfo.MapperClass?.Name}.g.cs", outputCode);
    }
}
