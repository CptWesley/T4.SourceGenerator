using Microsoft.CodeAnalysis;
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
            var escaped = e.ToString().Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
            ctx.ReportDiagnostic(Diagnostics.ExceptionError, file.GetLocation(0, 0), file.Path, escaped);
        }
    }
    /*
    private static void Unpack(StringBuilder sb, Exception e, int depth = 0)
    {
        if (e is AggregateException ae)
        {
            sb.AppendLine(ae.inn);
        }
    }

    private static void Print(StringBuilder sb, Exception e, int depth)
    {

    }

    private static string Pad(string text, int depth)
    {
        var padding = new string(' ', depth * 2);
        var lines = Regex.Split(text, @"\r\n|\r|\n");
        var modified = lines.Select(line => padding + line);
        var joined = string.Join(Environment.NewLine, )
    }
    */

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
        settings.CompilerOptions = "-nullable:enable";

#pragma warning disable
        var refs = generator.Refs.Select(x =>
        {
            if (x is null)
            {
                return "null";
            }
            else
            {
                return $"\"{x}\"";
            }
        });

        generator.Refs.Add(GetAssemblyName<Mono.TextTemplating.CompiledTemplate>());
        generator.Refs.Add(GetAssemblyName(typeof(Mono.TextTemplating.RoslynTemplatingEngineExtensions)));
        generator.Refs.Add(GetAssemblyName<System.CodeDom.CodeBinaryOperatorExpression>());
        generator.Refs.RemoveAll(x => string.IsNullOrWhiteSpace(x));

        File.WriteAllText($@"D:\Git\T4.SourceGenerator\src\T4.SourceGenerator\bin\Release\refs-{Guid.NewGuid().ToString("N")}.txt", string.Join(Environment.NewLine, refs));
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

    private static string GetAssemblyName(Type type)
    {
        var path = type.Assembly.Location?.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            path = type.Assembly.FullName;
        }
        return path;
    }

    private static string GetAssemblyName<T>()
        => GetAssemblyName(typeof(T));

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
