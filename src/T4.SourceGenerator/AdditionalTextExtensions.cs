using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace T4.SourceGenerator;

/// <summary>
/// Provides extension methods for <see cref="AdditionalText"/> instances.
/// </summary>
internal static class AdditionalTextExtensions
{
    /// <summary>
    /// Creates a new <see cref="Location"/> inside this <paramref name="file"/>.
    /// </summary>
    /// <param name="file">The base file.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="length">The length of the location.</param>
    /// <returns>The newly created <see cref="Location"/>.</returns>
    public static Location GetLocation(this AdditionalText file, int startIndex, int length)
        => Location.Create(file.Path, new TextSpan(startIndex, length), default);
}
