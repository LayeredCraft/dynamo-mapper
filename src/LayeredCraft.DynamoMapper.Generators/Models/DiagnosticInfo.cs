using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor DiagnosticDescriptor,
    LocationInfo? LocationInfo = null,
    params object[] MessageArgs
);

internal static class DiagnosticInfoExtensions
{
    extension(DiagnosticInfo diagnosticInfo)
    {
        internal Diagnostic ToDiagnostic() =>
            Diagnostic.Create(
                diagnosticInfo.DiagnosticDescriptor,
                diagnosticInfo.LocationInfo?.ToLocation(),
                diagnosticInfo.MessageArgs
            );

        internal void ReportDiagnostic(SourceProductionContext context) =>
            context.ReportDiagnostic(diagnosticInfo.ToDiagnostic());
    }
}
