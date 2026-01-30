using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HRConnect.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UsingStatementsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HC0002";
        private static readonly LocalizableString Title = "Using statements should be inside namespace";
        private static readonly LocalizableString MessageFormat = "Using statement should be inside the namespace declaration";
        private static readonly LocalizableString Description = "Enforces the style where all using statements are placed inside the namespace block rather than at the top of the file.";
        private const string Category = "Style";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken) as CompilationUnitSyntax;
            if (root == null)
                return;

            // Check if there are any using statements at the top level
            var topLevelUsings = root.Usings;
            if (topLevelUsings.Count == 0)
                return;

            // Check if there's a namespace
            var namespaceDecl = root.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

            // If there's no namespace or if using statements are outside the namespace, report diagnostic
            if (namespaceDecl == null || topLevelUsings.Count > 0)
            {
                foreach (var usingDirective in topLevelUsings)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        usingDirective.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
