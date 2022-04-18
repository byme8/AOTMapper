using System.Collections.Immutable;
using System.Linq;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AOTMapper
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OutputPropertiesAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(AOTMapperDescriptors.NotAllOutputValuesAreMapped);

        public override void Initialize(AnalysisContext context)
        {
#if !DEBUG
            context.EnableConcurrentExecution();
#endif
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.MethodDeclaration);
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax method))
            {
                return;
            }

            var returnType = context.SemanticModel.GetTypeInfo(method.ReturnType).Type;
            if (!(returnType is INamedTypeSymbol returnNamedType))
            {
                return;
            }

            var properties = returnNamedType
                .GetAllMembers()
                .OfType<IPropertySymbol>()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .ToArray();

            var memberAssignments = method.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Select(a => a.Left as MemberAccessExpressionSyntax)
                .Where(o => o != null)
                .Select(o => (Variable: o.Expression.ToString(), Property: o.Name.ToString()))
                .Where(o => o.Variable == "output")
                .Select(o => o.Property)
                .ToImmutableHashSet();

            var missingProperties = properties
                .Where(p => !memberAssignments.Contains(p.Name))
                .ToArray();
            
            if (!missingProperties.Any())
            {
                return;
            }

            var missingPropertiesString = string.Join(", ", missingProperties.Select(p => p.Name));
            var returnStatement = method.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Last();

            var diagnostic = Diagnostic.Create(AOTMapperDescriptors.NotAllOutputValuesAreMapped, returnStatement.GetLocation(), missingPropertiesString);
            context.ReportDiagnostic(diagnostic);
        }
    }
}