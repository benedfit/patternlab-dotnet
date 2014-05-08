---
layout: docs
title: Running the .NET version for the First Time | Pattern Lab
---

# Running the .NET version for the First Time

The .NET version of Pattern Lab differs some what from PHP version. At it's core it is an ASP.NET MVC powered dynamic site, with a static output generator. This means that once installed, it can be run by IIS Express directly from Visual Studio without the need for a setting up an additional web server.

Once you're ready to show off your edition of Pattern Lab, you can either point your clients directly at the ASP.NET MVC site, _or preferably_ generate a static site for no-hassle hosting.

## How to Generate the static output of Pattern Lab

To generate the static version of ASP.NET MVC website do the following:

* Visit `~/generate` in your browser

The site should now be generated in `public/` and available for hosting anywhere.