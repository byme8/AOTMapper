using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using AOTMapper.Core.Generators;
using System.Collections;

namespace AOTMapper
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AOTMapperCodeFixProvider)), Shared]
    public class AOTMapperCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(AOTMapperAnalyzer.AOTMapperIsNotReady);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(CodeAction.Create("Add mapper", token => this.Handle(diagnostic, context)), diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> Handle(Diagnostic diagnostic, CodeFixContext context)
        {
            var fromTypeName = diagnostic.Properties["fromType"];
            var toTypeName = diagnostic.Properties["toType"];
            var document = context.Document;

            var semanticModel = await document.GetSemanticModelAsync();
            
            var root = await diagnostic.Location.SourceTree.GetRootAsync();
            var call = root.FindNode(diagnostic.Location.SourceSpan);
            root = root.ReplaceNode(call, SyntaxFactory.IdentifierName($"MapTo{toTypeName.Split('.').Last()}"));

            var pairs = ImmutableDictionary<string, string>.Empty
                .Add(fromTypeName, toTypeName);

            var generator = new AOTMapperGenerator(document.Project, semanticModel.Compilation);
            generator.GenerateMappers(pairs, new[] { "AOTMapper", "Mappers" });

            var newProject = generator.Project;
            var oldDocumentInNewProject = newProject.GetDocument(document.Id);

            return oldDocumentInNewProject.WithSyntaxRoot(root);
        }
    }
}
