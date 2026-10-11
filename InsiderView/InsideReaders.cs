namespace InsiderView
{
    /// <summary>
    /// What is inside one project file: the names found, or the reason there are none.
    /// </summary>
    internal sealed class InsideResult
    {
        public IReadOnlyList<string> Names { get; }

        /// <summary>Why nothing could be read. Null when the file was read.</summary>
        public string? Problem { get; }

        private InsideResult(IReadOnlyList<string> names, string? problem)
        {
            Names = names;
            Problem = problem;
        }

        public static InsideResult Read(IReadOnlyList<string> names) => new(names, null);

        public static InsideResult Failed(string problem) => new(Array.Empty<string>(), problem);
    }

    /// <summary>
    /// Knows how to look inside each kind of project file. One line per file type: to teach the
    /// app a new type, write a method that takes a file path and returns the names inside it,
    /// and add it to the list below.
    /// </summary>
    internal static class InsideReaders
    {
        private static readonly Dictionary<string, Func<string, IReadOnlyList<string>>> ByExtension =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".ai"] = AiFile.TopLayerNames,
            };

        public static bool CanRead(string extension) => ByExtension.ContainsKey(extension);

        public static InsideResult Read(string path)
        {
            string extension = Path.GetExtension(path);
            if (!ByExtension.TryGetValue(extension, out Func<string, IReadOnlyList<string>>? read))
                return InsideResult.Failed($"Insider View cannot look inside {extension} files yet.");

            try
            {
                return InsideResult.Read(read(path));
            }
            catch (Exception ex)
            {
                return InsideResult.Failed("Could not be read: " + ex.Message);
            }
        }
    }
}
