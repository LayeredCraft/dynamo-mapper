using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Helpers;
using DynamoMapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoMapper.Generator;

internal static class MapperSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    internal static MapperInfo? Transformer(
        GeneratorAttributeSyntaxContext syntaxContext,
        CancellationToken cancellationToken
    )
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var context = new GeneratorContext(syntaxContext, cancellationToken);

            if (syntaxContext.TargetNode is not ClassDeclarationSyntax classDeclaration)
                return null;

            if (!classDeclaration.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
                return null;

            return MapperInfo.Create(classDeclaration, context);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is expected and should be rethrown
            throw;
        }
        catch (Exception ex)
        {
            // Return a minimal MapperInfo that contains the error
            return MapperInfo.CreateWithDiagnostics([
                new DiagnosticInfo(
                    DiagnosticDescriptors.InternalGeneratorError,
                    syntaxContext.TargetNode.GetLocation().CreateLocationInfo(),
                    ExceptionHelper.FormatExceptionMessage(ex)
                ),
            ]);
        }
    }
}
