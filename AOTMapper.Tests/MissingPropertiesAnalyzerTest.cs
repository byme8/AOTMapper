using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests;

public class MissingPropertiesAnalyzerTest
{
    [Fact]
    public async Task OneOfPropertiesIsNotAssigned()
    {
        var project = await TestProject.Project
            .Replace(("output.LastName = input.LastName;", ""));

        var diagnostics = await project.ApplyAnalyzers(new MissingPropertiesAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.NotAllOutputValuesAreMapped.Id);
    }

    [Fact]
    public async Task ReturnIsMissing()
    {
        var project = await TestProject.Project
            .Replace(("output", "result"));

        var diagnostics = await project.ApplyAnalyzers(new MissingPropertiesAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.ReturnOfOutputIsMissing.Id);
    }
}