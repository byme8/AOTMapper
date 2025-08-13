using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Analyzers;
using AOTMapper.CodeFixes;
using AOTMapper.Tests.Helpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AOTMapper.Tests.CodeFixes;

public class AddMissingPropertiesCodeFixTest
{
    [Fact]
    public async Task OneOfPropertiesIsNotAssigned()
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

        await TestProject.Project.VerifyCodeFix(mapper, new MissingPropertiesAnalyzer(), new AddMissingPropertiesCodeFix());
    }
    
    [Fact]
    public async Task MethodWithoutAssignments()
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
                         // Missing both: output.FirstName = input.FirstName;
                         // Missing both: output.LastName = input.LastName;
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

        await TestProject.Project.VerifyCodeFix(mapper, new MissingPropertiesAnalyzer(), new AddMissingPropertiesCodeFix());
    }
}