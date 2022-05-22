using AOTMapper.Core;
using AOTMapper.Tests.Data;
using Xunit;

namespace AOTMapper.Tests;

public class AOTMapperBuilderTests
{
    [Fact]
    public void CanAddMapper()
    {
        var mapper = AOTMapperBuilder.Create()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age
                })
            .Build();

        var user = new TestUser { Name = "John", Age = 30 };
        var userEntity = mapper.Map<TestUserEntity>(user);

        Assert.Equal(user.Name, userEntity.Name);
        Assert.Equal(user.Age, userEntity.Age);
    }

    [Fact]
    public void CanAddMultipleMappers()
    {
        var mapper = AOTMapperBuilder.Create()
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

        Assert.Equal(userRole.Name, userRoleEntity.Name);
    }

    [Fact]
    public void CanAddTransitiveMappers()
    {
        var mapper = AOTMapperBuilder.Create()
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

        Assert.Equal(user.Name, userEntity.Name);
        Assert.Equal(user.Age, userEntity.Age);
        Assert.Equal(user.Role.Name, userEntity.Role.Name);
    }

    [Fact]
    public void CanAddCombineMappers()
    {
        var mapper1 = AOTMapperBuilder.Create()
            .AddMapper<TestUser, TestUserEntity>((m, s)
                => new TestUserEntity
                {
                    Name = s.Name,
                    Age = s.Age,
                    Role = m.Map<TestUserRoleEntity>(s.Role)
                })
            .Build();

        var mapper2 = AOTMapperBuilder.Create()
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

        Assert.Equal(user.Name, userEntity.Name);
        Assert.Equal(user.Age, userEntity.Age);
        Assert.Equal(user.Role.Name, userEntity.Role.Name);
    }

    [Fact]
    public void MissingPropertiesAreIgnored()
    {
        var mapper = AOTMapperBuilder.Create()
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

        Assert.Equal(user.Name, userEntity.Name);
        Assert.Equal(0, userEntity.Age);
        Assert.Null(userEntity.Role);
    }
}