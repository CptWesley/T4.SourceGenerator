#pragma warning disable RS1035 // Used for initial proof of concept.

using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace T4.SourceGenerator;

/// <summary>
/// Provides extension methods for <see cref="GeneratorExecutionContext "/> instances.
/// </summary>
internal static class ContextExtensions
{
    /// <inheritdoc cref="GeneratorExecutionContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(this ref GeneratorExecutionContext ctx, DiagnosticDescriptor descriptor, Location location, params object[] args)
        => ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location, args));

    /// <inheritdoc cref="GeneratorExecutionContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(
        this ref GeneratorExecutionContext ctx,
        PositionFinder file,
        IEnumerable<CompilerError> messages)
    {
        foreach (var msg in messages)
        {
            ctx.ReportDiagnostic(file, msg);
        }
    }

    /// <inheritdoc cref="GeneratorExecutionContext.ReportDiagnostic(Diagnostic)" />
    public static void ReportDiagnostic(this ref GeneratorExecutionContext ctx, PositionFinder file, CompilerError msg)
    {
        var location = file.Find(msg.Line, msg.Column, msg.FileName);
        var descriptor = msg.IsWarning ? Diagnostics.ParseWarning : Diagnostics.ParseError;
        ctx.ReportDiagnostic(descriptor, location, msg.ErrorText);
    }

}
