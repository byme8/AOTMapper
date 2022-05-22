using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AOTMapper.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddMissingPropertiesCodeFixProvider)), Shared]
    public class AddMissingPropertiesCodeFixProvider : CodeFixProvider
    {
        public static string[] Spliter = new[] { OutputPropertiesAnalyzer.Spliter };

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AOTMapperDescriptors.NotAllOutputValuesAreMapped.Id);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(CodeAction.Create("Add missing properties", token => Handle(diagnostic, context)), diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> Handle(Diagnostic diagnostic, CodeFixContext context)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            if (root is null)
            {
                return context.Document;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync();
            var targetNode = root.FindNode(diagnostic.Location.SourceSpan);
            var methodsNode = targetNode.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (methodsNode is null)
            {
                return context.Document;
            }

            var maybeMethod = semanticModel.GetDeclaredSymbol(methodsNode);
            if (!(maybeMethod is IMethodSymbol method) || method.Parameters.IsEmpty)
            {
                return context.Document;
            }

            var inputParameter = method.Parameters.Last();
            var outputType = method.ReturnType;

            var allAssignments = targetNode
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .ToArray();

            var lastAssigment = allAssignments
                .LastOrDefault();

            if (lastAssigment == null)
            {
                return context.Document;
            }

            var outputProperties = outputType
                .GetAllPublicProperties()
                .ToDictionary(o => o.Name, o => o.Type);

            var assignmentProperties = allAssignments
                .Select(o => o.Left)
                .OfType<MemberAccessExpressionSyntax>()
                .Select(o => o.Name.Identifier.Text)
                .ToImmutableHashSet();

            var missingProperties = outputProperties
                .Where(o => !assignmentProperties.Contains(o.Key))
                .ToArray();

            var newAssignmentExpressions = missingProperties
                .Select(property => SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("output"),
                            SyntaxFactory.IdentifierName(property.Key)),
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(inputParameter.Name),
                            SyntaxFactory.IdentifierName(property.Key)))
                ))
                .ToArray();

            var statement = lastAssigment.Parent;
            if (statement is null)
            {
                return context.Document;
            }

            var newRoot = root.InsertNodesAfter(statement, newAssignmentExpressions);

            var newDocument = context.Document
                .WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}