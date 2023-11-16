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
                ctx.ReportDiagnostic(Diagnostics.UnknownError, file.GetLocation(0, 0), file.Path);
            }
        }
        catch (Exception e)
        {
            ctx.ReportDiagnostic(Diagnostics.ExceptionError, file.GetLocation(0, 0), file.Path, e.ToString());
        }
    }

    private static bool Generate(ref GeneratorExecutionContext ctx, AdditionalText file)
    {
        var generator = new TemplateGenerator();
        generator.UseInProcessCompiler();

        var templateName = Path.GetFileName(file.Path) ?? string.Empty;
        var templateContent = file.GetText()?.ToString() ?? string.Empty;
        var finder = PositionFinder.Create(templateContent, file.Path);

        var parsed = generator.ParseTemplate(templateName, templateContent);

        var messages = GetMessages(parsed.Errors);
        ctx.ReportDiagnostic(finder, messages);

        if (messages.Any(msg => !msg.IsWarning))
        {
            return true;
        }

        var settings = TemplatingEngine.GetSettings(generator, parsed);
        settings.CompilerOptions = "-nullable:enable";

        (var outputName, var content) = generator.ProcessTemplateAsync(parsed, templateName, templateContent, templateName, settings).Result;

        messages = GetMessages(generator.Errors);
        ctx.ReportDiagnostic(finder, messages);

        if (messages.Any(msg => !msg.IsWarning) || content is null || outputName is null)
        {
            return true;
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
