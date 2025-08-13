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
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntity(this IAOTMapper mapper, User input)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName;
                         // Missing: output.LastName = input.LastName;
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

        await TestProject.Project.VerifyAnalyzer(mapper, new MissingPropertiesAnalyzer());
    }
    
    [Fact]
    public async Task OneOfPropertiesIsMissingButValidationDisabled()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod(disableMissingPropertiesDetection: true)]
                     public static UserEntity MapUserToUserEntity(this IAOTMapper mapper, User input)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName;
                         // Missing: output.LastName = input.LastName;
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

        await TestProject.Project.VerifyAnalyzer(mapper, new MissingPropertiesAnalyzer());
    }

    [Fact]
    public async Task ReturnIsMissing()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntity(this IAOTMapper mapper, User input)
                     {
                         var result = new UserEntity();
                         result.FirstName = input.FirstName;
                         result.LastName = input.LastName;
                         return result; // Should return 'output', not 'result'
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

        await TestProject.Project.VerifyAnalyzer(mapper, new MissingPropertiesAnalyzer());
    }

    [Fact]
    public async Task MultiParameterInstanceExtension_DetectsMissingProperties()
    {
        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntityWithDefaults(this User input, string defaultFirstName)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName ?? defaultFirstName;
                         // Missing: LastName assignment
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

        await TestProject.Project.VerifyAnalyzer(mapper, new MissingPropertiesAnalyzer());
    }
}