namespace PatternLab.Core
{
    /// <summary>
    /// Defines a pattern engine supported by Pattern Lab
    /// </summary>
    public interface IPatternEngine
    {
        /// <summary>
        /// The file extension of pattern templates read by pattern engine
        /// </summary>
        /// <returns>The pattern engine extension</returns>
        string Extension();

        /// <summary>
        /// The name of the pattern engine
        /// </summary>
        /// <returns>The pattern engine name</returns>
        string Name();
    }
}