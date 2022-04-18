using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;

namespace AOTMapper
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FillMissingPropertiesCodeFixProvider)), Shared]
    public class FillMissingPropertiesCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(AOTMapperDescriptors.NotAllOutputValuesAreMapped.Id);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(CodeAction.Create("Add mappings", token => this.Handle(diagnostic, context)), diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> Handle(Diagnostic diagnostic, CodeFixContext context)
        {
            return context.Document;
        }
    }
}
