# CognitiveComplexityAnalyzer

Roslyn analyzer for [cognitive complexity](https://www.sonarsource.com/docs/CognitiveComplexity.pdf).

## Configuration

The default max complexity is 10.  Anything above 10 will result in an error. This setting can be modified by creating an [editorconfig](https://docs.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options?view=vs-2019) file. The setting is `dotnet_diagnostic.CC0001.max_cognitive_complexity`.
