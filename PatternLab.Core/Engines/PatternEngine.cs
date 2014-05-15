namespace PatternLab.Core
{
    public abstract class PatternEngine
    {
        public string Extension { get; private set; }

        public string Name { get; private set; }

        public PatternEngine(string name, string extension)
        {
            Name = name;
            Extension = extension;
        }
    }
}