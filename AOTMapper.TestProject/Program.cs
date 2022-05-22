using AOTMapper.Core;

namespace AOTMapper.TestProject;

public class Program
{
    public static void Main()
    {
        var mapper = new AOTMapperBuilder()
            .AddAOTMapperTestProject()
            .Build();

        var entity = mapper.Map<UserEntity>(new User
        {
            FirstName = "John",
            LastName = "Doe",
        });

        var user = mapper.Map<User>(entity);
    }
}

public static class Mapper
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