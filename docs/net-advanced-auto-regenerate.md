---
layout: docs
title: Watching for Changes and Auto Regenerating Patterns in the .NET version | Pattern Lab
---

# Watching for Changes and Auto Regenerating Patterns in the .NET version

Being dynamic, the .NET version of Pattern Lab automatically watches for changes to patterns and select files. When these files change, it will automatically rebuild the entire Pattern Lab website. You simply make your changes, save the file, and the .NET version of Pattern Lab will take care of the rest.

## The Default Files That Are Watched

By default, the .NET version of Pattern Lab monitors the following files:

* all of the patterns under `_patterns/`
* all of the JSON files under `_data/` 
* any directory without an `_` (underscore) or that doesn't match a directory name found in the `id` variable in `config/config.ini`
* any file with a file extension that doesn't match one found in the `ie` variable in `config/config.ini`

## Ignoring Other Directories & File Extensions 

Instructions on how to ignore assets in other directories or with other file extensions can be found in "[Managing Assets for a Pattern](/docs/pattern-managing-assets.html)".