# Contributing to OmoriSandbox

The README's Contributing section covers bug reports, feature requests and pull requests. This page covers setting up your editor for code contributions.

## Editing C# files

Godot's built-in script editor rewrites a file's indentation when it saves it. This repository mixes tab-indented and space-indented C# files, so saving a `.cs` file from inside Godot can change every line of it and bury your real change in the diff.

- Edit C# in an external editor or IDE, such as Visual Studio, Rider or VS Code. In Godot, set **Editor Settings › Dotnet › Editor › External Editor** so that scripts open there.
- If you do edit C# inside Godot, turn off **Editor Settings › Text Editor › Behavior › Files › Convert Indent on Save** and **Trim Trailing Whitespace on Save**.
- Before you push, drop whitespace-only changes. `git diff -w` shows your real changes, and `git add -p` lets you stage only those hunks.
