using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace T4.SourceGenerator;

/// <summary>
/// Provides extension methods for <see cref="SourceProductionContext "/> instances.
/// </summary>
internal static class ContextExtensions
{
    /// <inheritdoc cref="SourceProductionContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(this ref SourceProductionContext ctx, DiagnosticDescriptor descriptor, Location location, params object[] args)
        => ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location, args));

    /// <inheritdoc cref="SourceProductionContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(
        this ref SourceProductionContext ctx,
        PositionFinder file,
        IEnumerable<CompilerError> messages)
    {
        foreach (var msg in messages)
        {
            ctx.ReportDiagnostic(file, msg);
        }
    }

    /// <inheritdoc cref="SourceProductionContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(this ref SourceProductionContext ctx, PositionFinder file, CompilerError msg)
    {
        var location = file.Find(msg.Line, msg.Column, msg.FileName);
        var descriptor = msg.IsWarning ? Diagnostics.ParseWarning : Diagnostics.ParseError;
        ctx.ReportDiagnostic(descriptor, location, msg.ErrorText);
    }

}
