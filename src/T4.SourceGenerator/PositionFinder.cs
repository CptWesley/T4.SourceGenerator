using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace T4.SourceGenerator;

/// <summary>
/// Used for finding line positions in a file.
/// </summary>
public sealed class PositionFinder
{
    private readonly int[] lines;
    private readonly string fullPath;
    private readonly string shortPath;

    private PositionFinder(int[] lines, string shortPath, string fullPath)
    {
        this.lines = lines;
        this.shortPath = shortPath;
        this.fullPath = fullPath;
    }

    /// <summary>
    /// Gets the file index.
    /// </summary>
    /// <param name="line">The line number.</param>
    /// <param name="column">The column.</param>
    /// <returns>The found index.</returns>
    public int FindIndex(int line, int column)
    {
        if (line <= 0 || line > lines.Length)
        {
            return -1;
        }

        return lines[line - 1] + column - 1;
    }

    /// <summary>
    /// Gets the file location for the given <paramref name="line"/> and <paramref name="column"/>.
    /// </summary>
    /// <param name="line">The line number.</param>
    /// <param name="column">The column index.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The newly created <see cref="Location"/>.</returns>
    public Location Find(int line, int column, string? fileName)
    {
        var path = fileName ?? string.Empty;

        if (path == shortPath)
        {
            path = fullPath;
        }

        var pos = FindIndex(line, column);

        if (pos < 0)
        {
            return Location.Create(path, default, default);
        }

        var textSpan = new TextSpan(pos, 1);
        var start = new LinePosition(line, column);
        var end = new LinePosition(line, column);
        var lineSpan = new LinePositionSpan(start, end);
        return Location.Create(path, textSpan, lineSpan);
    }

    /// <summary>
    /// Creates a <see cref="PositionFinder"/> for the given <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The input file content.</param>
    /// <param name="path">The full path of the file.</param>
    /// <returns>The newly created <see cref="PositionFinder"/> instance.</returns>
    public static PositionFinder Create(string input, string path)
    {
        var result = new List<int>();
        var lengths = input
            .Split('\n')
            .Select(x => x.Length + 1);

        result.Add(0);

        foreach (var length in lengths)
        {
            var prev = result[result.Count - 1];
            result.Add(length + prev);
        }

        return new(result.ToArray(), Path.GetFileName(path), path);
    }
}
