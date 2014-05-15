namespace PatternLab.Core.Engines
{
    /// <summary>
    /// The Razor (.cshtml) pattern engine
    /// </summary>
    public class RazorPatternEngine : PatternEngine
    {
        /// <summary>
        /// Initialises a new Razor pattern engine
        /// </summary>
        public RazorPatternEngine() : base("razor", ".cshtml")
        {
        }
    }
}