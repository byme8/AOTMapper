namespace AOTMapper.Tests.Data;

class TestUser
{
    public string Name { get; set; }
    public int Age { get; set; }

    public TestUserRole Role { get; set; }
}

class TestUserEntity
{
    public string Name { get; set; }
    public int Age { get; set; }

    public TestUserRoleEntity Role { get; set; }
}

class TestUserRole
{
    public string Name { get; set; }
}

class TestUserRoleEntity
{
    public string Name { get; set; }
}