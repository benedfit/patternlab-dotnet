del /S *.nupkg

set opts=-Sym -Prop Configuration=Release -Build -IncludeReferencedProjects -OutputDirectory .

nuget pack ..\PatternLab\PatternLab.csproj %opts%
nuget pack ..\PatternLab.Razor\PatternLab.Razor.csproj %opts%
nuget pack ..\PatternLab.Core\PatternLab.Core.csproj %opts%
nuget pack ..\PatternLab.Core.Razor\PatternLab.Core.Razor.csproj %opts%
nuget pack ..\PatternLab.Starter\PatternLab.Starter.csproj -Exclude **/*.dll  %opts%
nuget pack ..\PatternLab.Starter.Razor\PatternLab.Starter.Razor.csproj -Exclude **/*.dll %opts%