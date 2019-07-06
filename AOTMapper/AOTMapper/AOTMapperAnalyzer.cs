using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AOTMapper
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AOTMapperAnalyzer : DiagnosticAnalyzer
    {
        private const string CoreAssemblyName = "AOTMapper.Core";

        public const string AOTMapperIsNotReady = "AOTMapperIsNotReady";

        public static DiagnosticDescriptor AOTMapperIsNotReadyDescriptor = new DiagnosticDescriptor(
                   AOTMapperIsNotReady,
                   "Mapper is not ready.",
                   "Mapper is not ready.",
                   "Codegen",
                   DiagnosticSeverity.Error,
                   isEnabledByDefault: true,
                   "Mapper is not ready.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.Create(AOTMapperIsNotReadyDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterOperationAction(this.Handle, OperationKind.Invocation);
        }

        private void Handle(OperationAnalysisContext context)
        {
            var syntax = context.Operation.Syntax;
            if (syntax is InvocationExpressionSyntax invocationSytax &&
                invocationSytax.Expression is MemberAccessExpressionSyntax memberAccessSyntax &&
                memberAccessSyntax.Expression is IdentifierNameSyntax identifireSyntax &&
                syntax.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault() is GenericNameSyntax genericNameSyntax &&
                genericNameSyntax.Identifier.ValueText == "MapTo")
            {
                var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                var methodInformation = semanticModel.GetSymbolInfo(genericNameSyntax);
                if (methodInformation.Symbol.ContainingAssembly.Name != CoreAssemblyName)
                {
                    return;
                }

                var fromTypeInfo = semanticModel.GetTypeInfo(identifireSyntax);
                var fromTypeName = fromTypeInfo.Type.ToDisplayString();

                var typeSyntax = genericNameSyntax.TypeArgumentList.Arguments.First();
                var toTypeInfo = semanticModel.GetTypeInfo(typeSyntax);
                var toTypeName = toTypeInfo.Type.ToDisplayString();

                var properties = ImmutableDictionary<string, string>.Empty
                    .Add("fromType", fromTypeName)
                    .Add("toType", toTypeName);

                context.ReportDiagnostic(Diagnostic.Create(AOTMapperIsNotReadyDescriptor, genericNameSyntax.GetLocation(), properties));
            }
        }
    }
}
