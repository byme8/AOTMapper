using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AOTMapper.Tests.Helpers;

public static class Utils
{
    public static async Task<ImmutableArray<Diagnostic>> ApplyAnalyzers(this Project project, params DiagnosticAnalyzer[] analyzers)
    {
        var compilation = await project.GetCompilationAsync();
        var compilationWithAnalyzers = compilation!.WithAnalyzers(ImmutableArray.Create(analyzers));
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        return diagnostics;
    }
    
    public static async Task<Project> Replace(this Project project, string oldText, string newText)
    {
        var document = project.Documents.Single(d => d.Name == "Program.cs");
        var textAsync = await document.GetTextAsync();
        var replacedText = textAsync.ToString().Replace(oldText, newText);
        var newDocument = document.WithText(SourceText.From(replacedText));

        return newDocument.Project;
    }
}