---
layout: docs
title: Requirements for .NET version | Pattern Lab
---
# Requirements for .NET version

The requirements for the .NET version of Pattern Lab vary depending on what features you want to use.

## Minimum Requirements for Building Pattern Lab

It's expected that you'll use the .NET version of Pattern Lab via **Visual Studio** locally on your computer to develop your atoms, molecules, organisms, templates and pages. To use the .NET version of Pattern Lab, you must have **.NET Framework 4.5**, with **ASP.NET MVC 4** installed.

You should _not_ need to set-up IIS or another web server to use Pattern Lab as it can be run by IIS Express directly from Visual Studio.

## Minimum Requirements for Hosting Pattern Lab

Once you want to show off your edition of Pattern Lab to a client you might want to put it on your web host. There are **no** requirements for hosting the [static output of the .NET version of Pattern Lab](/docs/net-first-run.html). The static site consists of HTML, CSS, and JavaScript. Simply upload the `public/` directory to your host and you should be good to go.