using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TextTemplating;
using Mono.TextTemplating;
using System.CodeDom.Compiler;

namespace T4.SourceGenerator;

/// <summary>
/// Provides source code generation for T4 templates.
/// </summary>
[Generator]
public sealed class T4Generator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.AdditionalTextsProvider
            .Where(f => Path.GetExtension(f.Path) == ".t4" || Path.GetExtension(f.Path) == ".tt");

        context.RegisterSourceOutput(provider, (ctx, file) =>
        {
            TryGenerate(ref ctx, file);
        });
    }

    private static void TryGenerate(ref SourceProductionContext ctx, AdditionalText file)
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
            var escaped = e.ToString().EscapeWhitespace();
            ctx.ReportDiagnostic(Diagnostics.ExceptionError, file.GetLocation(0, 0), file.Path, escaped);
        }
    }

    private static bool Generate(ref SourceProductionContext ctx, AdditionalText file)
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
        settings.LangVersion = "12.0";
        settings.CompilerOptions = "-nullable:enable";

        ConfigureRefs(generator);

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

    private static void ConfigureRefs(TemplateGenerator generator)
    {
        var refs = GetRefs();
        generator.Refs.Clear();
        generator.Refs.AddRange(refs);
    }

    private static HashSet<string> GetRefs()
    {
#pragma warning disable RS1035
        var refs = new HashSet<string>();

        // Standard library.
        refs.AddAssemblyName<string>();
        refs.AddAssemblyName<Uri>();
        refs.AddAssemblyName(typeof(File));
        refs.AddAssemblyName<StringReader>();
        refs.AddAssemblyName(typeof(Enumerable));
        refs.AddAssemblyName<MethodInfo>();

        // Visual studio
        refs.AddAssemblyName<TextTransformation>();

        // Mono.Templating
        refs.AddAssemblyName<CompiledTemplate>();
        refs.AddAssemblyName(typeof(RoslynTemplatingEngineExtensions));

        // System.CodeDom
        refs.AddAssemblyName<System.CodeDom.CodeBinaryOperatorExpression>();

#pragma warning restore RS1035
        return refs;
    }
}

/// <summary>Provides extension methods.</summary>
file static class Extensions
{
    /// <summary>
    /// Adds an assembly to the reference set based on the given <typeparamref name="T"/> marker type.
    /// </summary>
    /// <typeparam name="T">The marker type.</typeparam>
    /// <param name="refs">The references to add to.</param>
    internal static void AddAssemblyName<T>(this HashSet<string> refs)
        => refs.AddAssemblyName(typeof(T));

    /// <summary>
    /// Adds an assembly to the reference set based on the given marker <paramref name="type"/>.
    /// </summary>
    /// <param name="refs">The references to add to.</param>
    /// <param name="type">The marker type.</param>
    internal static void AddAssemblyName(this HashSet<string> refs, Type type)
    {
        var name = type.GetAssemblyName();
        if (name is { })
        {
            refs.Add(name);
        }
    }

    private static string? GetAssemblyName(this Type type)
    {
        var path = type.Assembly.Location?.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            path = type.Assembly.FullName;
        }

        return path;
    }
}
