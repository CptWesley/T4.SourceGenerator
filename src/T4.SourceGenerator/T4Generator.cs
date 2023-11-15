#pragma warning disable RS1035 // Used for initial proof of concept.

using Microsoft.CodeAnalysis;
using Mono.TextTemplating;
using System.CodeDom.Compiler;

namespace T4.SourceGenerator;

/// <summary>
/// Provides source code generation for T4 templates.
/// </summary>
[Generator]
public sealed class T4Generator : ISourceGenerator
{
    private static readonly DiagnosticDescriptor ParseError
        = new DiagnosticDescriptor("TF0001", "T4 Template parsing error", "{0}: {1}", "CodeGeneration", DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor ParseWarning
        = new DiagnosticDescriptor("TF0002", "T4 Template parsing warning", "{0}: {1}", "CodeGeneration", DiagnosticSeverity.Warning, true);

    private static readonly DiagnosticDescriptor ExceptionError
        = new DiagnosticDescriptor("TF0003", "T4 Template generation error", "Exception thrown while generating code for T4 template '{0}': {1}", "CodeGeneration", DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor UnknownError
        = new DiagnosticDescriptor("TF0004", "T4 Template generation error", "Unknown error occurred while generating code for T4 template '{0}'", "CodeGeneration", DiagnosticSeverity.Error, true);

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles.Where(f => Path.GetExtension(f.Path) == ".t4" || Path.GetExtension(f.Path) == ".tt"))
        {
            TryGenerate(ref context, file);
        }
    }

    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
        // Do nothing.
    }

    private static void TryGenerate(ref GeneratorExecutionContext ctx, AdditionalText file)
    {
        try
        {
            if (!Generate(ref ctx, file))
            {
                ctx.ReportDiagnostic(UnknownError, file.GetLocation(0, 0), file.Path);
            }
        }
        catch (Exception e)
        {
            ctx.ReportDiagnostic(ExceptionError, file.GetLocation(0, 0), file.Path, e.ToString());
        }
    }

    private static bool Generate(ref GeneratorExecutionContext ctx, AdditionalText file)
    {
        var generator = new TemplateGenerator();
        generator.UseInProcessCompiler();

        var templateName = Path.GetFileName(file.Path);
        var templateContent = file.GetText()?.ToString();
        var parsed = generator.ParseTemplate(templateName, templateContent);
        var messages = GetMessages(parsed.Errors);

        foreach (var msg in messages)
        {
            var location = file.GetLocation(0, 0);
            ctx.ReportDiagnostic(msg.IsWarning ? ParseWarning : ParseError, location, msg.ErrorNumber, msg.ErrorText);
        }

        if (messages.Any(msg => !msg.IsWarning))
        {
            return false;
        }

        var settings = TemplatingEngine.GetSettings(generator, parsed);
        settings.CompilerOptions = "-nullable:enable";

        (var outputName, var content) = generator.ProcessTemplateAsync(parsed, templateName, templateContent, templateName, settings).Result;

        messages = GetMessages(generator.Errors);
        foreach (var msg in messages)
        {
            var location = file.GetLocation(0, 0);
            ctx.ReportDiagnostic(msg.IsWarning ? ParseWarning : ParseError, location, msg.ErrorNumber, msg.ErrorText);
        }

        if (messages.Any(msg => !msg.IsWarning) || content is null || outputName is null)
        {
            return false;
        }

        ctx.AddSource(Path.GetFileName(outputName), content);
        return true;
    }

    private static IReadOnlyList<CompilerError> GetMessages(CompilerErrorCollection errors)
    {
        var result = new List<CompilerError>();
        for (var i = 0; i < errors.Count; i++)
        {
            result.Add(errors[i]);
        }

        return result;
    }
}
