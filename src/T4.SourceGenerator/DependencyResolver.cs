namespace T4.SourceGenerator;

/// <summary>
/// Provides a module initializer that configures the dependency resolver.
/// </summary>
internal static class DependencyResolver
{
    /// <summary>
    /// Registers the dependency resolver.
    /// </summary>
    //[ModuleInitializer]
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
        var name = new AssemblyName(fullName);
        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == name.FullName);
        if (loadedAssembly is { })
        {
            return loadedAssembly;
        }

        var currentAssembly = Assembly.GetExecutingAssembly();
        var resourceName = $"T4.SourceGenerator.{name.Name}.dll";

        using Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceName);

        var success = resourceStream is { };
#pragma warning disable
        File.WriteAllText(@$"D:\Git\T4.SourceGenerator\src\T4.SourceGenerator\bin\Release\foo-{Guid.NewGuid().ToString("N")}.txt", $"Looking for (Success={success}): '{resourceName}'. Found: [\n{string.Join("\n", currentAssembly.GetManifestResourceNames())}\n]");

        if (resourceStream is null)
        {
            return null;
        }

        using MemoryStream memoryStream = new MemoryStream();
        resourceStream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
#pragma warning disable RS1035
        var loaded = Assembly.Load(bytes);
#pragma warning restore RS1035
        return loaded;
    }
}
