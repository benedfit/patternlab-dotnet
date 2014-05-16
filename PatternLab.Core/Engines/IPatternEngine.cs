﻿using System.Collections.Generic;

namespace PatternLab.Core.Engines
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

        /// <summary>
        /// Parses a pattern template against a data collection using the pattern engine
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <param name="data">The data collection</param>
        /// <returns>The parsed string</returns>
        string Parse(Pattern pattern, Dictionary<string, object> data);
    }
}