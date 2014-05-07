set opts=-Prop Configuration=Release -Build -IncludeReferencedProjects -OutputDirectory .

nuget pack -sym ..\PatternLab.Source\PatternLab.Source.csproj %opts%