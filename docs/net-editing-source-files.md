---
layout: docs
title: Editing the Source Files of the .NET version | Pattern Lab
---

# Editing the Source Files of the .NET version

Because the .NET version of Pattern Lab is a static site generator you <u>**should not edit the files in the `public/` directory**</u>. Instead, you should edit the files directly in the root directory via Visual Studio, In addition to editing patterns under the `_patterns/` directory you'll want to [edit your JavaScript, CSS, and images](/docs/pattern-managing-assets.html) as well.

The .NET version of Pattern Lab will be automatically compiled after each change. However, `public/` will not be updated until the next time you [regenerate the static output of Pattern Lab](/docs/net-command-line.html).