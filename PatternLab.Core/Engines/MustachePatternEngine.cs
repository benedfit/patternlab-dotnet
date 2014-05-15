namespace PatternLab.Core.Engines
{
    /// <summary>
    /// The Mustache (.mustache) pattern engine
    /// </summary>
    public class MustachePatternEngine : PatternEngine
    {
        /// <summary>
        /// Initialises a new Mustache pattern engine
        /// </summary>
        public MustachePatternEngine() : base("mustache", ".mustache")
        {
        }
    }
}