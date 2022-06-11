using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AOTMapper.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddMissingPropertiesCodeFix)), Shared]
    public class AddMissingPropertiesCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AOTMapperDescriptors.MissingPropertiesDetected.Id);

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
            var methodNode = targetNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodNode is null)
            {
                return context.Document;
            }

            var maybeMethod = semanticModel.GetDeclaredSymbol(methodNode);
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

            var lastAssigment = allAssignments
                .LastOrDefault();

            if (lastAssigment == null)
            {
                return FillMethod();
            }

            return AddMissingAssignments();

            Document AddMissingAssignments()
            {
                var newAssignmentExpressions = missingProperties
                    .Select(property => SF.ExpressionStatement(
                        SF.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SF.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName("output"),
                                SF.IdentifierName(property.Key)),
                            SF.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName(inputParameter.Name),
                                SF.IdentifierName(property.Key)))
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

            Document FillMethod()
            {
                var outputVariable = SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(SF.ParseTypeName("var"),
                        SF.SeparatedList(
                            new[]
                            {
                                SF.VariableDeclarator(
                                    SF.Identifier("output"),
                                    null,
                                    SF.EqualsValueClause(
                                        SF.ObjectCreationExpression(
                                            SF.ParseTypeName(outputType.ToGlobalName()),
                                            SF.ArgumentList(),
                                            null)))
                            })));

                var newAssignmentExpressions = missingProperties
                    .Select(property => SF.ExpressionStatement(
                        SF.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SF.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName("output"),
                                SF.IdentifierName(property.Key)),
                            SF.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName(inputParameter.Name),
                                SF.IdentifierName(property.Key)))
                    ))
                    .ToArray();

                var returnOutputVariable = SF.ReturnStatement(
                    SF.IdentifierName("output"));

                var statements =
                    new StatementSyntax[] { outputVariable }
                        .Concat(newAssignmentExpressions)
                        .Concat(new[] { returnOutputVariable });

                var newRoot = root.ReplaceNode(methodNode, methodNode.WithBody(SF.Block(statements)));

                var newDocument = context.Document
                    .WithSyntaxRoot(newRoot);

                return newDocument;
            }
        }
    }
}