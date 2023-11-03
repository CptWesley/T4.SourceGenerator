using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace T4.SourceGenerator;

/// <summary>
/// Provides extension methods for <see cref="SourceGeneratorContext"/> instances.
/// </summary>
internal static class ContextExtensions
{
    /// <inheritdoc cref="SourceGeneratorContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(this ref SourceGeneratorContext ctx, DiagnosticDescriptor descriptor, Location location, params object[] args)
        => ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location, args));

    /// <inheritdoc cref="SourceGeneratorContext.AddSource(string, SourceText)" />
    public static void AddSource(this ref SourceGeneratorContext ctx, string hintName, string sourceText)
        => ctx.AddSource(hintName, SourceText.From(sourceText));
}
