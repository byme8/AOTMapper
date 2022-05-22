using System;
using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Core;
using AOTMapper.Tests.Helpers;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AOTMapper.Tests;

public class SourceGenerationTest
{
    [Fact]
    public async Task CompilesWithoutErrors()
    {
        var project = TestProject.Project;

        var compilation = await project.GetCompilationAsync();
        var errors = compilation.GetDiagnostics()
            .Where(o => o.Severity == DiagnosticSeverity.Error)
            .ToArray();
        
    }

    [Fact]
    public async Task AOTMapperExtensionWorks()
    {
        var assembly = await TestProject.Project.CompileToRealAssembly();
        var type = assembly.GetType("AOTMapper.Core.AOTMapperForAOTMapperTestProject");
        var method = type.GetMethod("AddAOTMapperTestProject")
            .CreateDelegate<Func<AOTMapperBuilder, AOTMapperBuilder>>();

        var builder = AOTMapperBuilder.Create();
        method.Invoke(builder);
        
        Assert.NotEmpty(builder.Descriptors);
    }
}