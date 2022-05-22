using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.CodeFixes;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AOTMapper.Tests;

public class CodeFixTest
{
    [Fact]
    public async Task OneOfPropertiesIsNotAssigned()
    {
        var project = await TestProject.Project
            .Replace(("output.LastName = input.LastName;", ""));

        var diagnostics = await project.ApplyAnalyzers(new OutputPropertiesAnalyzer());
        var diagnostic = diagnostics.Single(o => o.Id == AOTMapperDescriptors.NotAllOutputValuesAreMapped.Id);

        var newProject = await project.ApplyCodeFix(diagnostic, new AddMissingPropertiesCodeFixProvider());

         diagnostics =  await newProject.ApplyAnalyzers(new OutputPropertiesAnalyzer());

         diagnostics
             .Should().NotContain(o => o.Severity == DiagnosticSeverity.Error);
         diagnostics
             .Should().NotContain(o => o.Id == AOTMapperDescriptors.NotAllOutputValuesAreMapped.Id);
    }

}