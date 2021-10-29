# CognitiveComplexityAnalyzer

Roslyn analyzer for [cognitive complexity](https://www.sonarsource.com/docs/CognitiveComplexity.pdf).

Currently no support incrementing complexity for ternary operators yet.

Supports Visual Studio 2022.

## Configuration

Configuration is accomplished by adding and editing an [editorconfig](https://docs.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options?view=vs-2019) file to your solution.

### Max Complexity

The default max complexity is 10. The setting is `dotnet_diagnostic.CC0001.max_cognitive_complexity`. Anything above this setting will result in an warning. 

### Treat Warnings as Errors

There is an option to treat warnings as errors.  The settings is `dotnet_diagnostic.CC0001.treat_warnings_as_errors`.

