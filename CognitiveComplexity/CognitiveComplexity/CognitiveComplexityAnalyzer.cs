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
        public const string WarningDiagnosticId = "CC0001";
        public const string InfoDiagnosticId = "CC0002";
        private const string Category = "Complexity";
        private const string DotNetDiagnostic = "dotnet_diagnostic";
        private const string ComplexityLimitKey = "max_cognitive_complexity";

        private const int DefaultComplexityLimit = 10;

        private static readonly LocalizableString CC001Title = new LocalizableResourceString(nameof(Resources.HighCognitiveComplexity), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString CC002Title = new LocalizableResourceString(nameof(Resources.CognitiveComplexity), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString CognitiveComplexityMessage = new LocalizableResourceString(nameof(Resources.CognitiveComplexityMessge), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString HighCognitiveComplexityMessage = new LocalizableResourceString(nameof(Resources.HighCognitiveComplexityMessage), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(InfoDiagnosticId, CC002Title, CognitiveComplexityMessage, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor WarningRule = new DiagnosticDescriptor(WarningDiagnosticId, CC001Title, HighCognitiveComplexityMessage, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(WarningRule, InfoRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(this.AnalyzeMethodComplexity);
        }

        private void AnalyzeMethodComplexity(SyntaxTreeAnalysisContext context)
        {
            var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            config.TryGetValue($"{DotNetDiagnostic}.{WarningDiagnosticId}.{ComplexityLimitKey}", out var configValue);
            var maxCC = int.TryParse(configValue, out var _maxCC) ? _maxCC : DefaultComplexityLimit;

            var root = context.Tree.GetRoot(context.CancellationToken);
            foreach(var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var walker = new MethodSyntaxCCWalker();
                var complexity = walker.GetComplexity(method);
                var rule = complexity > maxCC ? WarningRule : InfoRule;
                var diagnostic = Diagnostic.Create(rule, method.Identifier.GetLocation(), complexity);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
