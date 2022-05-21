using System.Threading.Tasks;
using AOTMapper.Diagnostics;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests;

public class AnalyzerTest
{
    [Fact]
    public async Task OneOfPropertiesIsNotAssigned()
    {
        var project = await TestProject.Project
            .Replace(("output.LastName = input.LastName;", ""));

        var diagnostics = await project.ApplyAnalyzers(new OutputPropertiesAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.NotAllOutputValuesAreMapped.Id);
    }

    [Fact]
    public async Task ReturnIsMissing()
    {
        var project = await TestProject.Project
            .Replace(("output", "result"));

        var diagnostics = await project.ApplyAnalyzers(new OutputPropertiesAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.ReturnOfOutputIsMissing.Id);
    }
}