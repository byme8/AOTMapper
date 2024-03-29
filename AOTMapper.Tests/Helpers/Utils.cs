using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AOTMapper.Tests.Helpers;

public static class Utils
{
    public static async Task<ImmutableArray<Diagnostic>> ApplyAnalyzers(this Project project, params DiagnosticAnalyzer[] analyzers)
    {
        var compilation = await project.GetCompilationAsync();
        var compilationWithAnalyzers = compilation!.WithAnalyzers(ImmutableArray.Create(analyzers));
        var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();
        return diagnostics;
    }

    public static async Task<Project> ApplyCodeFix(this Project project, Diagnostic diagnostic, CodeFixProvider fix)
    {
        var document = project.Solution.GetDocument(diagnostic.Location.SourceTree);
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);

        await fix.RegisterCodeFixesAsync(context);

        var operations = await actions.First().GetOperationsAsync(CancellationToken.None);
        var changeSolution = operations.OfType<ApplyChangesOperation>().First().ChangedSolution;
        var newProject = changeSolution.Projects.First();

        return newProject;
    }

    public static async Task<Project> Replace(this Project project, params (string OldText, string NewText)[] changes)
    {
        var document = project.Documents.Single(d => d.Name == "Program.cs");
        var textAsync = await document.GetTextAsync();
        var text = textAsync.ToString();
        var newText = changes.Aggregate(text, (current, change) => current.Replace(change.OldText, change.NewText));
        var newDocument = document.WithText(SourceText.From(newText));

        return newDocument.Project;
    }
    
    public static async Task<Assembly> CompileToRealAssembly(this Project project)
    {
        var compilation = await project.GetCompilationAsync();
        var analyzerResults = compilation.GetDiagnostics();

        var error = compilation.GetDiagnostics().Concat(analyzerResults)
            .FirstOrDefault(o => o.Severity == DiagnosticSeverity.Error);

        if (error != null)
        {
            throw new Exception(error.GetMessage());
        }

        using (var memoryStream = new MemoryStream())
        {
            compilation.Emit(memoryStream);
            var bytes = memoryStream.ToArray();
            var assembly = Assembly.Load(bytes);

            return assembly;
        }
    }
}