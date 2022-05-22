using AOTMapper.Core;
using AOTMapper.Tests.Data;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests;

public class AOTMapperBuilderTests
{
    [Fact]
    public void CanAddMapper()
    {
        var mapper = new AOTMapperBuilder()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age
                })
            .Build();

        var user = new TestUser { Name = "John", Age = 30 };
        var userEntity = mapper.Map<TestUserEntity>(user);

        userEntity.Name.Should().Be(user.Name);
        userEntity.Age.Should().Be(user.Age);
    }

    [Fact]
    public void CanAddMultipleMappers()
    {
        var mapper = new AOTMapperBuilder()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age
                })
            .AddMapper<TestUserRole, TestUserRoleEntity>((m, s)
                => new TestUserRoleEntity
                {
                    Name = s.Name
                })
            .Build();

        var userRole = new TestUserRole()
        {
            Name = "Admin",
        };

        var userRoleEntity = mapper.Map<TestUserRoleEntity>(userRole);
        
        userRole.Name.Should().Be(userRoleEntity.Name);
    }

    [Fact]
    public void CanAddTransitiveMappers()
    {
        var mapper = new AOTMapperBuilder()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age,
                    Role = m.Map<TestUserRoleEntity>(s.Role)
                })
            .AddMapper<TestUserRole, TestUserRoleEntity>((m, s)
                => new TestUserRoleEntity
                {
                    Name = s.Name
                })
            .Build();

        var user = new TestUser { Name = "John", Age = 30, Role = new TestUserRole { Name = "Admin" } };
        var userEntity = mapper.Map<TestUserEntity>(user);

        userEntity.Name.Should().Be(user.Name);
        userEntity.Age.Should().Be(user.Age);
        userEntity.Role.Name.Should().Be(user.Role.Name);
    }

    [Fact]
    public void CanAddCombineMappers()
    {
        var mapper1 = new AOTMapperBuilder()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age,
                    Role = m.Map<TestUserRoleEntity>(s.Role)
                })
            .Build();

        var mapper2 = new AOTMapperBuilder()
            .AddMapper<TestUserRole, TestUserRoleEntity>((m, s)
                => new TestUserRoleEntity
                {
                    Name = s.Name
                })
            .Build();

        var mapper = AOTMapperBuilder
            .FromMappers(mapper1, mapper2)
            .Build();

        var user = new TestUser { Name = "John", Age = 30, Role = new TestUserRole { Name = "Admin" } };
        var userEntity = mapper.Map<TestUserEntity>(user);

        userEntity.Name.Should().Be(user.Name);
        userEntity.Age.Should().Be(user.Age);
        userEntity.Role.Name.Should().Be(user.Role.Name);
    }

    [Fact]
    public void MissingPropertiesAreIgnored()
    {
        var mapper = new AOTMapperBuilder()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age,
                    Role = m.Map<TestUserRoleEntity>(s.Role)
                })
            .AddMapper<TestUserRole, TestUserRoleEntity>((m, s)
                => new TestUserRoleEntity
                {
                    Name = s.Name
                })
            .Build();

        var user = new TestUser { Name = "John" };
        var userEntity = mapper.Map<TestUserEntity>(user);

        userEntity.Name.Should().Be(user.Name);
        userEntity.Age.Should().Be(0);
        userEntity.Role.Should().BeNull();
    }
}