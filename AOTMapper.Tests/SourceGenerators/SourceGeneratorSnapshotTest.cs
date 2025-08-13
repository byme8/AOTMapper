using System.Threading.Tasks;
using Xunit;
using AOTMapper.Tests.Helpers;

namespace AOTMapper.Tests.SourceGenerators;

public class SourceGeneratorSnapshotTest
{
    [Fact]
    public async Task ClassicMapperPattern_GeneratesCorrectCode()
    {
        const string source = @"
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
}";

        await TestProject.Project.VerifySource(source);
    }

    [Fact]
    public async Task InstanceExtensionPattern_GeneratesCorrectCode()
    {
        const string source = @"
using AOTMapper;
using AOTMapper.Core;

namespace TestProject
{
    public static class UserMappers
    {
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
}";

        await TestProject.Project.VerifySource(source);
    }

    [Fact]
    public async Task BothPatterns_GeneratesCorrectCode()
    {
        const string source = @"
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
}";

        await TestProject.Project.VerifySource(source);
    }

    [Fact]
    public async Task MultipleTypes_GeneratesCorrectCode()
    {
        const string source = @"
using AOTMapper;
using AOTMapper.Core;

namespace TestProject
{
    public static class Mappers
    {
        [AOTMapperMethod]
        public static ProductEntity MapProductToEntity(this IAOTMapper mapper, Product input)
        {
            var output = new ProductEntity();
            output.Name = input.Name;
            output.Price = input.Price;
            return output;
        }

        [AOTMapperMethod]
        public static CustomerDto MapCustomerToDto(this Customer input)
        {
            var output = new CustomerDto();
            output.Name = input.Name;
            output.Email = input.Email;
            return output;
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class CustomerDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}";

        await TestProject.Project.VerifySource(source);
    }

    [Fact]
    public async Task MultiParameterInstanceExtensions_AreNotRegistered()
    {
        const string source = @"
using AOTMapper;
using AOTMapper.Core;

namespace TestProject
{
    public static class UserMappers
    {
        // Classic pattern - should be registered
        [AOTMapperMethod]
        public static UserEntity MapUserToUserEntity(this IAOTMapper mapper, User input)
        {
            var output = new UserEntity();
            output.FirstName = input.FirstName;
            output.LastName = input.LastName;
            return output;
        }

        // Single parameter instance extension - should be registered
        [AOTMapperMethod]
        public static User MapUserEntityToUser(this UserEntity input)
        {
            var output = new User();
            output.FirstName = input.FirstName;
            output.LastName = input.LastName;
            return output;
        }

        // Multi-parameter instance extension - should NOT be registered
        [AOTMapperMethod]
        public static UserEntity MapUserToUserEntityWithDefaults(this User input, string defaultFirstName, string defaultLastName)
        {
            var output = new UserEntity();
            output.FirstName = input.FirstName ?? defaultFirstName;
            output.LastName = input.LastName ?? defaultLastName;
            return output;
        }

        // Another multi-parameter instance extension - should NOT be registered
        [AOTMapperMethod]  
        public static UserDto MapUserToDto(this User input, bool includeFullName)
        {
            var output = new UserDto();
            output.Name = includeFullName ? $""{input.FirstName} {input.LastName}"" : input.FirstName;
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
}";

        await TestProject.Project.VerifySource(source);
    }
}