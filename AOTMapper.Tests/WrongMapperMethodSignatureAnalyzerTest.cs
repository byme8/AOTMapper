using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests;

public class WrongMapperMethodSignatureAnalyzerTest
{
    [Fact]
    public async Task MapperParameterIsMissing()
    {
        var project = await TestProject.Project
            .Replace(("this IAOTMapper mapper,", ""));

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task FirstParameterHasWrongType()
    {
        var project = await TestProject.Project
            .Replace(("this IAOTMapper mapper,", "this object mapper,"));

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task ObjectToMapIsMissing()
    {
        var project = await TestProject.Project
            .Replace((", User input", ""));

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task TooManyArguments()
    {
        var project = await TestProject.Project
            .Replace((", User input", ", User input, User input2"));

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task MethodReturnsVoid()
    {
        var project = await TestProject.Project
            .Replace(("static UserEntity", "static void"));

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
}