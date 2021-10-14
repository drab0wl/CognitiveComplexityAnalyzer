using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace CognitiveComplexity
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CognitiveComplexityAnalyzer : DiagnosticAnalyzer
    {
        public const string ErrorDiagnosticId = "CC0001";
        public const string InfoDiagnosticId = "CC0002";
        private const string Category = "Complexity";
        private const string ComplexityLimitKey = "max_cognitive_complexity";
        private const int DefaultComplexityLimit = 10;
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor(ErrorDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(InfoDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(ErrorRule, InfoRule); } }

        public override void Initialize(AnalysisContext context)
        {
            Debugger.Launch();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(this.AnalyzeMethodComplexity);
        }

        private void AnalyzeMethodComplexity(SyntaxTreeAnalysisContext context)
        {
            var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            config.TryGetValue($"dotnet_diagnostic.{ErrorDiagnosticId}.{ComplexityLimitKey}", out var configValue);
            var maxCC = int.TryParse(configValue, out var _maxCC) ? _maxCC : DefaultComplexityLimit;

            var root = context.Tree.GetRoot(context.CancellationToken);
            foreach(var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var walker = new SyntaxTreeWalker();
                var complexity = walker.GetComplexity(method);
                var rule = complexity > maxCC ? ErrorRule : InfoRule;
                var diagnostic = Diagnostic.Create(rule, method.Identifier.GetLocation(), complexity);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
