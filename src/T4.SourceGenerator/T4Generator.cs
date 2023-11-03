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
    public void Execute(SourceGeneratorContext context)
    {
        var generator = new TemplateGenerator();
        foreach (var file in context.AdditionalFiles.Where(f => Path.GetExtension(f.Path) == ".t4" || Path.GetExtension(f.Path) == ".tt"))
        {
            TryGenerate(ref context, generator, file);
        }
    }

    /// <inheritdoc />
    public void Initialize(InitializationContext context)
    {
        // Do nothing.
    }

    private static void TryGenerate(ref SourceGeneratorContext ctx, TemplateGenerator generator, AdditionalText file)
    {
        try
        {
            if (!Generate(ref ctx, generator, file))
            {
                ctx.ReportDiagnostic(UnknownError, file.GetLocation(0, 0), file.Path);
            }
        }
        catch (Exception e)
        {
            ctx.ReportDiagnostic(ExceptionError, file.GetLocation(0, 0), file.Path, e.ToString());
        }
    }

    private static bool Generate(ref SourceGeneratorContext ctx, TemplateGenerator generator, AdditionalText file)
    {
        var templateName = Path.GetFileName(file.Path);
        var templateContent = file.GetText()?.ToString();
        var parsed = generator.ParseTemplate(templateName, templateContent);
        var messages = GetMessages(parsed);

        foreach (var msg in messages)
        {
            var location = file.GetLocation(0, 0);
            ctx.ReportDiagnostic(msg.IsWarning ? ParseWarning : ParseError, location, msg.ErrorNumber, msg.ErrorText);
        }

        if (messages.Any(msg => !msg.IsWarning))
        {
            return true;
        }

        var settings = TemplatingEngine.GetSettings(generator, parsed);
        settings.CompilerOptions = "-nullable:enable";

        (var outputName, var content, var success) = generator.ProcessTemplateAsync(templateName, templateContent, templateName).Result;

        if (success)
        {
            ctx.AddSource(outputName, content);
        }

        return success;
    }

    private static IEnumerable<CompilerError> GetMessages(ParsedTemplate parsed)
    {
        for (var i = 0; i < parsed.Errors.Count; i++)
        {
            yield return parsed.Errors[i];
        }
    }
}
