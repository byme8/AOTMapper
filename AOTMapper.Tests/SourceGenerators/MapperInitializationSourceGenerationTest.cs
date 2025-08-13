using System;
using System.Linq;
using System.Threading.Tasks;
using AOTMapper.Core;
using AOTMapper.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AOTMapper.Tests.SourceGenerators;

public class MapperInitializationSourceGenerationTest
{
    [Fact]
    public async Task CompilesWithoutErrors()
    {
        var project = TestProject.Project;

        var compilation = await project.GetCompilationAsync();
        var errors = compilation.GetDiagnostics()
            .Where(o => o.Severity == DiagnosticSeverity.Error)
            .ToArray();
        
        Assert.Empty(errors);
        
    }

    [Fact]
    public async Task AOTMapperExtensionWorks()
    {
        var source = """
                   var builder = new AOTMapperBuilder()
                        .AddAOTMapperTestProject();
                        
                   return builder.Descriptors; 
                   """;

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
        
        await TestProject.Project.VerifyExecute(source, mapper);
    }

    [Fact]
    public async Task InstanceExtensionMethodMapperWorks()
    {
        var source = """
                   var builder = new AOTMapperBuilder()
                        .AddAOTMapperTestProject();
                   var mapper = builder.Build();
                   
                   // Test instance extension method mapping
                   var user = new global::TestProject.User { FirstName = "John", LastName = "Doe" };
                   var userEntity = mapper.Map<global::TestProject.UserEntity>(user);
                   var mappedUser = mapper.Map<global::TestProject.User>(userEntity);
                   
                   return $"{mappedUser.FirstName},{mappedUser.LastName}";
                   """;

        var mapper = """
             using AOTMapper;
             using AOTMapper.Core;
             
             namespace TestProject
             {
                 public static class UserMappers
                 {
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntity(this User input)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName;
                         output.LastName = input.LastName;
                         return output;
                     }
                     
                     [AOTMapperMethod]
                     public static User MapUserEntityToUser(this UserEntity input)
                     {
                         var output = new User();
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
        
        await TestProject.Project.VerifyExecute(source, mapper);
    }
}