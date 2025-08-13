using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests.Analyzers;

public class WrongMapperMethodSignatureAnalyzerTest
{
    [Fact]
    public async Task MapperParameterIsMissing()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntity(User input)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName;
                         output.LastName = input.LastName;
                         return output;
                     }
                 }
             
                 public class User
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             
                 public class UserEntity  
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             }
             """;

        var project = TestProject.Project
            .AddDocument("Test.cs", mapper)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task ObjectToMapIsMissing()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntity()
                     {
                         var output = new UserEntity();
                         output.FirstName = "Test";
                         output.LastName = "Test";
                         return output;
                     }
                 }
             
                 public class User
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             
                 public class UserEntity  
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             }
             """;

        var project = TestProject.Project
            .AddDocument("Test.cs", mapper)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task TooManyArguments()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntity(this IAOTMapper mapper, User input, User input2)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName;
                         output.LastName = input.LastName;
                         return output;
                     }
                 }
             
                 public class User
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             
                 public class UserEntity  
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             }
             """;

        var project = TestProject.Project
            .AddDocument("Test.cs", mapper)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
    
    [Fact]
    public async Task MethodReturnsVoid()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static void MapUserToUserEntity(this IAOTMapper mapper, User input)
                     {
                         // This should fail because it returns void
                     }
                 }
             
                 public class User
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             
                 public class UserEntity  
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             }
             """;

        var project = TestProject.Project
            .AddDocument("Test.cs", mapper)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        diagnostics.Should().Contain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }

    [Fact]
    public async Task MultiParameterInstanceExtensions_AreValid()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     // Multi-parameter instance extension - should be valid (no diagnostics)
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntityWithDefaults(this User input, string defaultFirstName, string defaultLastName)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName ?? defaultFirstName;
                         output.LastName = input.LastName ?? defaultLastName;
                         return output;
                     }

                     // Another multi-parameter instance extension with different types
                     [AOTMapperMethod]  
                     public static UserDto MapUserToDto(this User input, bool includeFullName, string separator)
                     {
                         var output = new UserDto();
                         output.Name = includeFullName ? $"{input.FirstName}{separator}{input.LastName}" : input.FirstName;
                         return output;
                     }
                 }
             
                 public class User
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }
             
                 public class UserEntity  
                 {
                     public string FirstName { get; set; }
                     public string LastName { get; set; }
                 }

                 public class UserDto
                 {
                     public string Name { get; set; }
                 }
             }
             """;

        var project = TestProject.Project
            .AddDocument("Test.cs", mapper)
            .Project;

        var diagnostics = await project.ApplyAnalyzers(new AOTMapperMethodValidationAnalyzer());

        // Should have no diagnostics - multi-parameter instance extensions are valid
        diagnostics.Should().NotContain(o => o.Id == AOTMapperDescriptors.AOTMapperMethodWrongDeclaration.Id);
    }
}