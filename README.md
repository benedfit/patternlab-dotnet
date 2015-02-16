## NOTICE

Please note, development has stalled on the .NET version of Pattern Lab. The current version contains all of the core functionality from v0.7.12 of the PHP version.

## Call for contributors

Anyone interested in becoming a contributor please contact [@benedfit](https://twitter.com/benedfit)

## About the .NET version of Pattern Lab

* [Pattern Lab Website](http://patternlab.io/)
* [About Pattern Lab](http://patternlab.io/about.html)
* [Documentation](http://patternlab.io/docs/index.html)
* [Demo](http://demo.patternlab.io/)

The .NET version of Pattern Lab differs some what from PHP version. At it's core, it combines the [Mustache](http://mustache.github.io/)-based patterns and the JavaScript-based viewer, with a ASP.NET MVC powered dynamic site and "builder" that transforms and dynamically builds the Pattern Lab site. By making it a static site generator, Pattern Lab strongly separates patterns, data, and presentation from build logic.

## Demo

You can play with a demo of the front-end of Pattern Lab at [demo.patternlab.io](http://demo.patternlab.io).

## Getting Started

**Quick start via NuGet Package Manager Console:**

*Latest stable release: 0.7.12.7*

```
PM> Install-Package PatternLab
```

* [Requirements](https://github.com/pattern-lab/patternlab-net/blob/master/docs/net-requirements.md)
* [Installing the .NET Version of Pattern Lab](https://github.com/pattern-lab/patternlab-net/blob/master/docs/net-installation.md)
* [Running the .NET version for the First Time](https://github.com/pattern-lab/patternlab-net/blob/master/docs/net-first-run.md)
* [Editing the Source Files of the .NET version](https://github.com/pattern-lab/patternlab-net/blob/master/docs/net-editing-source-files.md)
* [Using The Static Output Generator](https://github.com/pattern-lab/patternlab-net/blob/master/docs/net-command-line.md)

## Working with Patterns

Patterns are the core element of Pattern Lab. Understanding how they work is the key to getting the most out of the system. Patterns use [Mustache](http://mustache.github.io/) so please read [Mustache's docs](http://mustache.github.io/mustache.5.html) as well.

* [How Patterns Are Organized](http://patternlab.io/docs/pattern-organization.html)
* [Adding New Patterns](http://patternlab.io/docs/pattern-add-new.html)
* [Reorganizing Patterns](http://patternlab.io/docs/pattern-reorganizing.html)
* [Including One Pattern Within Another via Partials](http://patternlab.io/docs/pattern-including.html)
* [Managing Assets for a Pattern: JavaScript, images, CSS, etc.](http://patternlab.io/docs/pattern-managing-assets.html)
* [Modifying the Pattern Header and Footer](http://patternlab.io/docs/pattern-header-footer.html)
* [Using Pseudo-Patterns](http://patternlab.io/docs/pattern-pseudo-patterns.html)
* [Using Pattern Parameters](http://patternlab.io/docs/pattern-parameters.html)
* [Using Pattern State](http://patternlab.io/docs/pattern-states.html)
* [Using styleModifiers](http://patternlab.io/docs/pattern-stylemodifier.html)
* ["Hiding" Patterns in the Navigation](http://patternlab.io/docs/pattern-hiding.html)
* [Adding Annotations](http://patternlab.io/docs/pattern-adding-annotations.html)
* [Viewing Patterns on a Mobile Device](http://patternlab.io/docs/pattern-mobile-view.html)

## Creating & Working With Dynamic Data for a Pattern

The .NET version of Pattern Lab utilizes Mustache as the template language for patterns. In addition to allowing for the [inclusion of one pattern within another](http://patternlab.io/docs/pattern-including.html) it also gives pattern developers the ability to include variables. This means that attributes like image sources can be centralized in one file for easy modification across one or more patterns. The .NET version of Pattern Lab uses a JSON file, `_data/data.json`, to centralize many of these attributes.

* [Introduction to JSON & Mustache Variables](http://patternlab.io/docs/data-json-mustache.html)
* [Overriding the Central `data.json` Values with Pattern-specific Values](http://patternlab.io/docs/data-pattern-specific.html)
* [Linking to Patterns with Pattern Lab's Default `link` Variable](http://patternlab.io/docs/data-link-variable.html)
* [Creating Lists with Pattern Lab's Default `listItems` Variable](http://patternlab.io/docs/data-listitems.html)

## Using Pattern Lab's Advanced Features

* [Watching for Changes and Auto Regenerating Patterns in the .NET version](https://github.com/pattern-lab/patternlab-net/blob/master/docs/net-advanced-auto-regenerate.md)
* [Keyboard Shortcuts](http://patternlab.io/docs/advanced-keyboard-shortcuts.html)
* [Special Pattern Lab-specific Query String Variables ](http://patternlab.io/docs/pattern-linking.html)
* [Preventing the Cleaning of public/](http://patternlab.io/docs/advanced-clean-public.html)
* [Modifying the Pattern Lab Nav](http://patternlab.io/docs/advanced-pattern-lab-nav.html)
* [Editing the config.ini Options](http://patternlab.io/docs/advanced-config-options.html)
