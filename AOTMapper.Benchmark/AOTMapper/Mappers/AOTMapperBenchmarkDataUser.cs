using System;
using AOTMapper.Benchmark.Data;
using AOTMapper.Core;

namespace AOTMapper.Benchmark.AOTMapper.Mappers;

public static class AotMapperBenchmarkDataUserExtensions 
{
    [AOTMapperMethod]
    public static UserEntity MapToUserEntity(this User input)
    {
        var output = new UserEntity();
        output.FirstName = input.FirstName;
        output.LastName = input.LastName;
        output.Id = Guid.Empty;

        return output;
    }
}
