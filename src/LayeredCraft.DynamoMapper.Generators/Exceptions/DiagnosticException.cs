namespace DynamoMapper.Generator.Exceptions;

/// <summary>Custom Exception used to bubble up a diagnostic</summary>
internal class DiagnosticException(DiagnosticInfo diagnosticInfo) : Exception
{
    internal DiagnosticInfo DiagnosticInfo { get; } = diagnosticInfo;
}
