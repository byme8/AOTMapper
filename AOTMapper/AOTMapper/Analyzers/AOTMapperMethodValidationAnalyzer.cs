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
    public class AOTMapperMethodValidationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(AOTMapperDescriptors.AOTMapperMethodWrongDeclaration);

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
            if (!(context.Node is MethodDeclarationSyntax method) || method.GetAOTMapperMethodAttribute() == null)
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null)
            {
                return;
            }

            var attribute = methodSymbol.GetAttributes()
                .FirstOrDefault(o => o.AttributeClass?.ToGlobalName() == "global::AOTMapper.Core.AOTMapperMethodAttribute")?
                .AttributeClass;

            if (attribute is null)
            {
                return;
            }

            var parameters = methodSymbol.Parameters;

            var isValidSignature = IsValidSignature(methodSymbol, parameters);

            if (!isValidSignature)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        AOTMapperDescriptors.AOTMapperMethodWrongDeclaration,
                        method.Identifier.GetLocation()));
                return;
            }

            if (methodSymbol.ReturnsVoid)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        AOTMapperDescriptors.AOTMapperMethodWrongDeclaration,
                        method.Identifier.GetLocation()));
                return;
            }
        }

        private static bool IsValidSignature(IMethodSymbol methodSymbol, ImmutableArray<IParameterSymbol> parameters)
        {
            const string aotMapperType = "global::AOTMapper.IAOTMapper";

            if (!methodSymbol.IsExtensionMethod)
            {
                return false;
            }

            var firstParamType = parameters.First().Type.ToGlobalName();
            var hasTwoParameters = parameters.Length == 2; 

            if (firstParamType == aotMapperType && hasTwoParameters)
            {
                return true; // (IAOTMapper, T) => R
            }

            if (firstParamType == aotMapperType)
            {
                return false; // (IAOTMapper, <any this>) => R
            }

            return true;
        }
    }
}