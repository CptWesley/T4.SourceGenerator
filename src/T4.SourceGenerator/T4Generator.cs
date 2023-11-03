#pragma warning disable RS1035 // PoC for now.

using Microsoft.CodeAnalysis;
using Mono.TextTemplating;

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
        var generator = new TemplateGenerator();
        foreach (var file in context.AdditionalFiles.Where(f => Path.GetExtension(f.Path) == ".t4" || Path.GetExtension(f.Path) == ".tt"))
        {
            Generate(ref context, generator, file);
        }
    }

    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
        // Do nothing.
    }

    private static bool TryGenerate(ref GeneratorExecutionContext ctx, TemplateGenerator generator, AdditionalText file)

    private static bool Generate(ref GeneratorExecutionContext ctx, TemplateGenerator generator, AdditionalText file)
    {
        var templateName = Path.GetFileName(file.Path);
        var templateContent = file.GetText()?.ToString();
        var parsed = generator.ParseTemplate(templateName, templateContent);
        var settings = TemplatingEngine.GetSettings(generator, parsed);

        settings.CompilerOptions = "-nullable:enable";

        (var outputName, var content, var success) = generator.ProcessTemplateAsync(templateName, templateContent, templateName).Result;

        if (success)
        {
            ctx.AddSource(outputName, content);
        }

        return success;
    }
}
