using AOTMapper.Core;
using AOTMapper.Tests.Data;
using FluentAssertions;
using Xunit;

namespace AOTMapper.Tests
{
    public class AOTMapperTests
    {
        [Fact]
        public void CanPinType()
        {
            var mapper = new AOTMapperBuilder()
                .AddMapper<IUser, TestUserEntity>((m, s)
                    => new TestUserEntity
                    {
                        Name = s.Name,
                        Age = s.Age
                    })
                .Build();

            var user = new TestUser { Name = "John", Age = 30 };
            var userEntity = mapper.Map<TestUserEntity, IUser>(user);

            userEntity.Name.Should().Be(user.Name);
            userEntity.Age.Should().Be(user.Age);
        }
    }
}