using System.Threading.Tasks;
using AOTMapper.Tests.Helpers;
using Xunit;

namespace AOTMapper.Tests
{
    public class MultiParameterExtensionTest
    {
        [Fact]
        public async Task MultiParameterExtensions_WorkAsExtensionsButNotInMapper()
        {
            var source = """
                       
                       var builder = new AOTMapperBuilder()
                            .AddAOTMapperTestProject();
                       var mapper = builder.Build();

                       var user = new User { FirstName = "John", LastName = "Doe" };
                       var entity = mapper.Map<UserEntity>(user);  // Classic pattern
                       var mappedUser = mapper.Map<User>(entity);  // Single-param instance pattern
                       
                       var userWithDefaults = user.MapUserToUserEntityWithDefaults("Default1", "Default2");
                       var userDto = user.MapUserToDto(true, "-");

                       var registeredCount = builder.Descriptors.Count;
                       
                       return new {
                        builder,
                        user,
                        entity,
                        mappedUser,
                        userWithDefaults,
                        userDto
                       };
                       """;

            var mapper = """
                 using AOTMapper;
                 using AOTMapper.Core;
                 
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

                     // Single-parameter instance extension - registered with mapper
                     [AOTMapperMethod]
                     public static User MapUserEntityToUser(this UserEntity input)
                     {
                         var output = new User();
                         output.FirstName = input.FirstName;
                         output.LastName = input.LastName;
                         return output;
                     }

                     // Multi-parameter instance extension - NOT registered, but available as extension
                     [AOTMapperMethod]
                     public static UserEntity MapUserToUserEntityWithDefaults(this User input, string defaultFirstName, string defaultLastName)
                     {
                         var output = new UserEntity();
                         output.FirstName = input.FirstName ?? defaultFirstName;
                         output.LastName = input.LastName ?? defaultLastName;
                         return output;
                     }

                     // Another multi-parameter instance extension - NOT registered, but available as extension
                     [AOTMapperMethod]  
                     public static UserDto MapUserToDto(this User input, bool includeFullName, string separator)
                     {
                         var output = new UserDto();
                         output.Name = includeFullName ? (input.FirstName + separator + input.LastName) : input.FirstName;
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
                 """;
            
            await TestProject.Project.VerifyExecute(source, mapper);
        }
    }
}