using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AOTMapper.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OutputPropertiesAnalyzer : DiagnosticAnalyzer
    {
        public const string MissingProperties = "MissingProperties";
        public const string Spliter = ", ";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                AOTMapperDescriptors.NotAllOutputValuesAreMapped,
                AOTMapperDescriptors.ReturnOfOutputIsMissing);

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
            if (!(context.Node is MethodDeclarationSyntax method) || !method.CanBeAOTMapperMethod())
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

            if (missingProperties.Any())
            {
                var missingPropertiesString = string.Join(Spliter, missingProperties.Select(p => p.Name));
                var diagnosticProperties = new Dictionary<string, string>
                    {
                        { MissingProperties, missingPropertiesString }
                    }
                    .ToImmutableDictionary();

                var diagnosticOnReturn = Diagnostic.Create(
                    AOTMapperDescriptors.NotAllOutputValuesAreMapped,
                    method.Identifier.GetLocation(),
                    properties: diagnosticProperties,
                    missingPropertiesString);

                context.ReportDiagnostic(diagnosticOnReturn);
            }

            var returnStatement = method.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .LastOrDefault();

            if (returnStatement != null)
            {
                if (returnStatement.Expression?.ToString() != "output")
                {
                    var diagnostic = Diagnostic.Create(
                        AOTMapperDescriptors.ReturnOfOutputIsMissing,
                        method.Identifier.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}