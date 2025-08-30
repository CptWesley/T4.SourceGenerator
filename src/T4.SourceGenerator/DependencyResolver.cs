#pragma warning disable RS1035
namespace T4.SourceGenerator;

/// <summary>
/// Provides a module initializer that configures the dependency resolver.
/// </summary>
internal static class DependencyResolver
{
    private static readonly HashSet<string> Tried = new();

    /// <summary>
    /// Registers the dependency resolver.
    /// </summary>
    [ModuleInitializer]
    public static void RegisterDependencyResolver()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            return Load(args.Name);
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Assembly? Load(string fullName)
    {
        if (!Tried.Add(fullName))
        {
            return null;
        }

        var name = new AssemblyName(fullName);
        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == name.FullName);
        if (loadedAssembly is { })
        {
            return loadedAssembly;
        }

        var currentAssembly = Assembly.GetExecutingAssembly();
        var resourceName = $"T4.SourceGenerator.{name.Name}.dll";

        using Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceName);
        if (resourceStream is null)
        {
            return null;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "T4.SourceGenerator", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempDir);
        var fileName = Path.Combine(tempDir, $"{name.Name}.dll");
        using (var fs = File.Create(fileName))
        {
            resourceStream.CopyTo(fs);
            fs.Flush();
        }

        var asm = Assembly.LoadFile(fileName);
        return asm;
    }
}
