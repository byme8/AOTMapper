
using System;

public static class AOTMapperBenchmarkDataUserExtentions 
{
    public static AOTMapper.Benchmark.Data.UserEntity MapToUserEntity(this AOTMapper.Benchmark.Data.User input)
    {
        var output = new AOTMapper.Benchmark.Data.UserEntity();
        output.FirstName = input.FirstName;
        output.LastName = input.LastName;
        output.Id = Guid.Empty;

        return output;
    }
}
