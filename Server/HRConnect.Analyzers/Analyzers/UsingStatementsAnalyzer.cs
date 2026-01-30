namespace HRConnect.Analyzers
{
  using System.Collections.Immutable;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Microsoft.CodeAnalysis.Diagnostics;

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
      if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
        return;

      // Check if there are any using statements at the top level (file level)
      var topLevelUsings = root.Usings;
      if (topLevelUsings.Count == 0)
        return;

      // Check if there's a namespace in the file
      var namespaceDecl = root.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

      // Only report if there ARE top-level using statements AND there IS a namespace
      // This means using statements should be moved inside the namespace
      if (namespaceDecl != null && topLevelUsings.Count > 0)
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
