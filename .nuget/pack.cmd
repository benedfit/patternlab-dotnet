del /S *.nupkg

set opts=-Prop Configuration=Release -Build -OutputDirectory .

nuget pack -sym ..\PatternLab\PatternLab.csproj %opts%
nuget pack -sym ..\PatternLab.Core\PatternLab.Core.csproj %opts%
nuget pack -sym ..\PatternLab.Starter.Mustache\PatternLab.Starter.Mustache.csproj -Exclude **\*.dll %opts%
nuget pack -sym ..\PatternLab.Starter.Razor\PatternLab.Starter.Razor.csproj -Exclude **\*.dll %opts%