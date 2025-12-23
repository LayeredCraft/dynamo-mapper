using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace DynamoMapper.Generator;

internal readonly struct DiagnosticInfo(
    DiagnosticDescriptor diagnosticDescriptor,
    LocationInfo locationInfo,
    params object[] messageArgs
) : IEquatable<DiagnosticInfo>
{
    public DiagnosticDescriptor DiagnosticDescriptor { get; } = diagnosticDescriptor;
    public LocationInfo LocationInfo { get; } = locationInfo;
    public object[] MessageArgs { get; } = messageArgs;

    public bool Equals(DiagnosticInfo other) =>
        DiagnosticDescriptor.Id == other.DiagnosticDescriptor.Id
        && LocationInfo == other.LocationInfo;

    public override int GetHashCode() =>
        HashCode.Combine(DiagnosticDescriptor.Id.GetHashCode(), LocationInfo.GetHashCode());
}

internal static class DiagnosticInfoExtensions
{
    extension(DiagnosticInfo diagnosticInfo)
    {
        internal Diagnostic CreateDiagnostic() =>
            Diagnostic.Create(
                diagnosticInfo.DiagnosticDescriptor,
                diagnosticInfo.LocationInfo.ToLocation(),
                diagnosticInfo.MessageArgs
            );

        internal void ReportDiagnostic(SourceProductionContext context) =>
            context.ReportDiagnostic(diagnosticInfo.CreateDiagnostic());
    }
}
