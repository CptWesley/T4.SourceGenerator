using Microsoft.CodeAnalysis;

namespace T4.SourceGenerator;

/// <summary>
/// Holds diagnostic definitions.
/// </summary>
public static class Diagnostics
{
    /// <summary>
    /// Indicates a parse error.
    /// </summary>
    public static readonly DiagnosticDescriptor ParseError
        = new DiagnosticDescriptor("TF0001", "T4 Template parsing error", "{0}", "CodeGeneration", DiagnosticSeverity.Error, true);

    /// <summary>
    /// Indicates a parse warning.
    /// </summary>
    public static readonly DiagnosticDescriptor ParseWarning
        = new DiagnosticDescriptor("TF0002", "T4 Template parsing warning", "{0}", "CodeGeneration", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Indicates an exception was thrown.
    /// </summary>
    public static readonly DiagnosticDescriptor ExceptionError
        = new DiagnosticDescriptor("TF0003", "T4 Template generation error", "Exception thrown while generating code for T4 template '{0}': {1}", "CodeGeneration", DiagnosticSeverity.Error, true);

    /// <summary>
    /// Indicates an unknown failure occurred.
    /// </summary>
    public static readonly DiagnosticDescriptor UnknownError
        = new DiagnosticDescriptor("TF0004", "T4 Template generation error", "Unknown error occurred while generating code for T4 template '{0}'", "CodeGeneration", DiagnosticSeverity.Error, true);
}
