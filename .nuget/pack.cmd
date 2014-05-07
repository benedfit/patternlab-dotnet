set opts=-Prop Configuration=Release -Build -OutputDirectory .

nuget pack -sym ..\PatternLab\PatternLab.csproj %opts%
nuget pack -sym ..\PatternLab.Core\PatternLab.Core.csproj %opts%