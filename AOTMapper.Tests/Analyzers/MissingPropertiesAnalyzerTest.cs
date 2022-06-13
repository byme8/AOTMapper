using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests.Analyzers;

public class MissingPropertiesAnalyzerTest
{
    [Fact]
    public async Task OneOfPropertiesIsMissing()
    {
        var project = await TestProject.Project
            .Replace(("output.LastName = input.LastName;", ""));

        var diagnostics = await project.ApplyAnalyzers(new MissingPropertiesAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.MissingPropertiesDetected.Id);
    }
    
    [Fact]
    public async Task OneOfPropertiesIsMissingButValidationDisabled()
    {
        var project = await TestProject.Project
            .Replace(
                ("[AOTMapperMethod]", "[AOTMapperMethod(disableMissingPropertiesDetection: true)]"),
                ("output.LastName = input.LastName;", ""));

        var diagnostics = await project.ApplyAnalyzers(new MissingPropertiesAnalyzer());

        diagnostics.Should().NotContain(o => o.Id == AOTMapperDescriptors.MissingPropertiesDetected.Id);
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