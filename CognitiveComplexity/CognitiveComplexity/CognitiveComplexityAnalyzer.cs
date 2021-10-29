using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CognitiveComplexity
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CognitiveComplexityAnalyzer : DiagnosticAnalyzer
    {
        public const string ErrorDiagnosticId = "CC0001";
        public const string InfoDiagnosticId = "CC0002";
        public const string WarningDiagnosticId = "CC0003";
        private const string Category = "Complexity";
        private const string DotNetDiagnostic = "dotnet_diagnostic";
        private const string ComplexityLimitKey = "max_cognitive_complexity";
        private const string ErrorsAsWarningsKey = "treat_warnings_as_errors";
        private const int DefaultComplexityLimit = 10;
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor(ErrorDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(InfoDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor WarningRule = new DiagnosticDescriptor(WarningDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(ErrorRule, WarningRule, InfoRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(this.AnalyzeMethodComplexity);
        }

        private void AnalyzeMethodComplexity(SyntaxTreeAnalysisContext context)
        {
            var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            config.TryGetValue($"{DotNetDiagnostic}.{ErrorDiagnosticId}.{ComplexityLimitKey}", out var configValue);
            var maxCC = int.TryParse(configValue, out var _maxCC) ? _maxCC : DefaultComplexityLimit;

            config.TryGetValue($"{DotNetDiagnostic}.{ErrorDiagnosticId}.{ErrorsAsWarningsKey}", out configValue);
            var treatWarningAsError = bool.TryParse(configValue, out var _tEaW) ? _tEaW : false;

            var root = context.Tree.GetRoot(context.CancellationToken);
            foreach(var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var walker = new MethodSyntaxCCWalker();
                var complexity = walker.GetComplexity(method);
                var overMaxRule = treatWarningAsError ? ErrorRule : WarningRule;
                var rule = complexity > maxCC ? overMaxRule : InfoRule;
                var diagnostic = Diagnostic.Create(rule, method.Identifier.GetLocation(), complexity);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
