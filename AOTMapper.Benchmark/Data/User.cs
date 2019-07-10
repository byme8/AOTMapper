using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace AOTMapper.Benchmark.Data
{
    public class UserEntity
    {
        public UserEntity()
        {

        }

        public UserEntity(Guid id, string firstName, string lastName)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Name { get; set; }
    }
}
