using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AOTMapper.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using VerifyXunit;

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

    public static async Task VerifySource(this Project project, string source)
    {
        project = project
            .AddDocument("Test.cs", source)
            .Project;
        
        var compilation = await project.GetCompilationAsync();

        var generator = new AOTMapperModuleInitializerSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        
        var result = driver.RunGenerators(compilation!);
        var runResult = result.GetRunResult();
        
        var generatedSources = runResult.Results
            .SelectMany(r => r.GeneratedSources)
            .Select(gs => new { FileName = gs.HintName, Content = gs.SourceText.ToString() })
            .ToArray();

        var diagnostics = runResult.Diagnostics
            .Select(d => new { d.Id, d.Severity, Message = d.GetMessage() })
            .ToArray();

        await Verifier.Verify(new
        {
            GeneratedSources = generatedSources,
            Diagnostics = diagnostics
        });
    }

    public static async Task VerifyExecute(this Project project, string main, params string[] additionalFiles)
    {
        project = await project.Replace(("return null!; // replace to execute something", main));

        for (int i = 0; i < additionalFiles.Length; i++)
        {
            var file = additionalFiles[i];
            
            project = project
                .AddDocument($"{i}.cs", file)
                .Project;
        }
        
        var assembly = await project.CompileToRealAssembly();

        var program = assembly.GetType("AOTMapper.TestProject.Program")!;
        var executeMethod = program
            .GetMethod("Execute", BindingFlags.Static | BindingFlags.Public)!
            .CreateDelegate<Func<object>>();
        
        var result = executeMethod();

        await Verifier.Verify(result);
    }

    public static async Task VerifyAnalyzer<T>(this Project project, string source, T analyzer) where T : DiagnosticAnalyzer
    {
        project = project
            .AddDocument("Test.cs", source)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(analyzer);

        // Filter to only show diagnostics from our analyzer (not compiler warnings)
        var relevantDiagnostics = diagnostics
            .Where(d => analyzer.SupportedDiagnostics.Any(supported => supported.Id == d.Id))
            .Select(d => new { d.Id, d.Severity, Message = d.GetMessage() })
            .ToArray();

        await Verifier.Verify(new
        {
            Diagnostics = relevantDiagnostics
        });
    }

    public static async Task VerifyCodeFix<TAnalyzer, TCodeFix>(this Project project, string source, TAnalyzer analyzer, TCodeFix codeFix) 
        where TAnalyzer : DiagnosticAnalyzer 
        where TCodeFix : CodeFixProvider
    {
        project = project
            .AddDocument("Test.cs", source)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(analyzer);
        var relevantDiagnostics = diagnostics
            .Where(d => analyzer.SupportedDiagnostics.Any(supported => supported.Id == d.Id))
            .ToArray();

        if (!relevantDiagnostics.Any())
        {
            await Verifier.Verify(new
            {
                OriginalDiagnostics = relevantDiagnostics.Select(d => new { d.Id, d.Severity, Message = d.GetMessage() }).ToArray(),
                FixedCode = "No diagnostics to fix",
                FixedDiagnostics = Array.Empty<object>()
            });
            return;
        }

        // Get original source code
        var originalDocument = project.Documents.FirstOrDefault(d => d.Name == "Test.cs");
        var originalSourceText = await originalDocument.GetTextAsync();
        var originalCode = originalSourceText.ToString();

        var diagnostic = relevantDiagnostics.First();
        var newProject = await project.ApplyCodeFix(diagnostic, codeFix);
        
        // Get the fixed source code
        var fixedDocument = newProject.Documents.FirstOrDefault(d => d.Name == "Test.cs");
        var fixedSourceText = await fixedDocument.GetTextAsync();
        var fixedCode = fixedSourceText.ToString();

        // Check diagnostics after fix
        var fixedDiagnostics = await newProject.ApplyAnalyzers(analyzer);
        var fixedRelevantDiagnostics = fixedDiagnostics
            .Where(d => analyzer.SupportedDiagnostics.Any(supported => supported.Id == d.Id))
            .Select(d => new { d.Id, d.Severity, Message = d.GetMessage() })
            .ToArray();

        await Verifier.Verify(new
        {
            OriginalDiagnostics = relevantDiagnostics.Select(d => new { d.Id, d.Severity, Message = d.GetMessage() }).ToArray(),
            OriginalCode = originalCode,
            FixedCode = fixedCode,
            FixedDiagnostics = fixedRelevantDiagnostics
        });
    }
}