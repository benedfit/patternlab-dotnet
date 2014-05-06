set opts=-Prop Configuration=Release -Build -IncludeReferencedProjects -OutputDirectory .

nuget pack ..\PatternLab.Source\PatternLab.Source.csproj %opts%