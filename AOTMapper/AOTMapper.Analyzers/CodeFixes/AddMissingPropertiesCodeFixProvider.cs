using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Diagnostics;
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
            var missingProperties = diagnostic
                .Properties[OutputPropertiesAnalyzer.MissingProperties]
                .Split(Spliter, StringSplitOptions.RemoveEmptyEntries);

            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var returnStatement = root
                .FindNode(diagnostic.Location.SourceSpan)
                .FirstAncestorOrSelf<ReturnStatementSyntax>();

            if (returnStatement == null)
            {
                return context.Document;
            }

            var newAssignmentExpressions = missingProperties
                .Select(property => SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("output"),
                            SyntaxFactory.IdentifierName(property)),
                        SyntaxFactory.IdentifierName(" // .."))
                ))
                .ToArray();
            
            var newRoot = root.InsertNodesBefore(returnStatement, newAssignmentExpressions);

            var newDocument = context.Document
                .WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}