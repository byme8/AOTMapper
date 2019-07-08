using System;
using System.Collections.Generic;
using System.Text;

namespace AOTMapper.Benchmark.Data
{
    public class Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class UserEntity : Entity
    {
        public string FirstName { get; set; } = Guid.NewGuid().ToString();
        public string LastName { get; set; } = Guid.NewGuid().ToString();
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Name { get; set; }
    }
}
