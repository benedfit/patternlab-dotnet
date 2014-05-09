---
layout: docs
title: Installation of .NET version | Pattern Lab
---

# Installation of .NET version

Installing the .NET version of Pattern Lab should be fairly painless. First make sure you have the [required versions of the .NET Framework and ASP.NET MVC](/docs/net-requirements.html) and then:

1. Create a new ASP.NET Empty Web Application in Visual Studio.
2. Install Pattern Lab via **NuGet**; by either [searching](http://docs.nuget.org/docs/start-here/managing-nuget-packages-using-the-dialog) for **Pattern Lab**, or using the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) command;
	
		PM> Install-Package PatternLab.

3. [Run Pattern Lab for the first time](/docs/net-first-run.html).