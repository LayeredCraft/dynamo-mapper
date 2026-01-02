using System.Reflection;
using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Helpers;
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
        try
        {
            var model = new
            {
                GeneratedCodeAttribute,
                mapperInfo.MapperClass,
                ModelClass = mapperInfo.ModelClass!,
                MapperClassNamespace = mapperInfo.MapperClass?.Namespace,
                MapperClassSignature = mapperInfo.MapperClass?.ClassSignature,
                DictionaryCapacity = mapperInfo.ModelClass!.ToAttributeAssignments.Count,
            };

            var outputCode = TemplateHelper.Render("Templates.Mapper.scriban", model);

            context.AddSource($"{mapperInfo.MapperClass?.Name}.g.cs", outputCode);
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("template", StringComparison.OrdinalIgnoreCase))
        {
            // Template-specific errors
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TemplateError,
                mapperInfo.MapperClass?.Location?.ToLocation(),
                ex.Message
            );
            context.ReportDiagnostic(diagnostic);
        }
        catch (Exception ex)
        {
            // Generic generation errors
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.InternalGeneratorError,
                mapperInfo.MapperClass?.Location?.ToLocation(),
                ExceptionHelper.FormatExceptionMessage(ex)
            );
            context.ReportDiagnostic(diagnostic);
        }
    }
}
