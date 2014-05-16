namespace PatternLab.Core.Engines
{
    /// <summary>
    /// The Mustache (.mustache) pattern engine
    /// </summary>
    public class MustachePatternEngine : IPatternEngine
    {
        /// <summary>
        /// The file extension of pattern templates read by pattern engine
        /// </summary>
        /// <returns>.cshtml</returns>
        public string Extension()
        {
            return ".mustache";
        }

        /// <summary>
        /// The name of the pattern engine
        /// </summary>
        /// <returns>Razor</returns>
        public string Name()
        {
            return "Mustache";
        }
    }
}