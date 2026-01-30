namespace HRConnect.Analyzers
{
  using System.Collections.Immutable;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Microsoft.CodeAnalysis.Diagnostics;

  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class SampleAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "HC0001";
    private static readonly LocalizableString Title = "Avoid method name 'BadMethod'";
    private static readonly LocalizableString MessageFormat = "Method '{0}' uses forbidden name 'BadMethod'";
    private static readonly LocalizableString Description = "Demonstration analyzer: flags methods named 'BadMethod'.";
    private const string Category = "Naming";

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
      context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
      var methodDecl = (MethodDeclarationSyntax)context.Node;
      var name = methodDecl.Identifier.Text;
      if (string.Equals(name, "BadMethod", System.StringComparison.Ordinal))
      {
        var diagnostic = Diagnostic.Create(Rule, methodDecl.Identifier.GetLocation(), name);
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
