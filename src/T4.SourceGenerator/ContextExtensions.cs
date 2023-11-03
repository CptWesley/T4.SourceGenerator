#pragma warning disable RS1035 // Used for initial proof of concept.

using Microsoft.CodeAnalysis;

namespace T4.SourceGenerator;

/// <summary>
/// Provides extension methods for <see cref="GeneratorExecutionContext "/> instances.
/// </summary>
internal static class ContextExtensions
{
    /// <inheritdoc cref="GeneratorExecutionContext .ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(this ref GeneratorExecutionContext ctx, DiagnosticDescriptor descriptor, Location location, params object[] args)
        => ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location, args));
}
