using System.Linq;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace AOTMapper.Tests.Helpers;

public static class TestProject
{
    public static Project Project;
    
    static TestProject()
    {
        var manager = new AnalyzerManager();
        manager.GetProject(@"../../../../AOTMapper.TestProject/AOTMapper.TestProject.csproj");
        var workspace = manager.GetWorkspace();

        Project = workspace.CurrentSolution.Projects.First(o => o.Name == "AOTMapper.TestProject");
    }
}