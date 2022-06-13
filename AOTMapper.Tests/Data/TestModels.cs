namespace AOTMapper.Tests.Data;

public interface IUser
{
    string Name { get; set; }

    int Age { get; set; }
}

public class TestUser : IUser
{
    public string Name { get; set; }
    public int Age { get; set; }

    public TestUserRole Role { get; set; }
}

public class TestUserEntity
{
    public string Name { get; set; }
    public int Age { get; set; }

    public TestUserRoleEntity Role { get; set; }
}

public class TestUserRole
{
    public string Name { get; set; }
}

public class TestUserRoleEntity
{
    public string Name { get; set; }
}