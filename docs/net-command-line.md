---
layout: docs
title: Using The Static Output Generator | Pattern Lab
---

# Using The Static Output Generator

Unlike the PHP version the .NET version of Pattern Lab does makes use of the browser for generation of static output rather than a command line interface.

## The Generate Command and Options

The generate command generates an entire site a single time. By default it removes old content in `public/`, compiles the patterns and moves content from `_patterns/` into `public/patterns/`. Options can be mixed and matched.

    Usage:
      ~/generate? [&patternsonly=boolean] [&nocache=boolean] [&enablecss=boolean] 
    
    Available parameters:
      patternsonly    Generate only the patterns. Does NOT clean public/.
      nocache         Set the cacheBuster value to 0.
      enablecss       Generate CSS for each pattern. Resource intensive. **This feature is not currently supported in the .NET version.**
    
    Samples:
    
     To generate only the patterns:
	   ~/generate?patternsonly=true
    
     To turn off the cacheBuster:
       ~/generate?nocache=true
    
     To run and generate the CSS for each pattern:
       ~/generate?enablecss=true