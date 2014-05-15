namespace PatternLab.Core
{
    /// <summary>
    /// Defines a pattern engine supported by Pattern Lab
    /// </summary>
    public abstract class PatternEngine
    {
        /// <summary>
        /// The file extension of pattern templates read by pattern engine
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// The name of the pattern engine
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initialises a new pattern engine
        /// </summary>
        /// <param name="name">The name of the pattern engine</param>
        /// <param name="extension">The file extension of pattern templates read by pattern engine</param>
        public PatternEngine(string name, string extension)
        {
            Name = name;
            Extension = extension;
        }
    }
}