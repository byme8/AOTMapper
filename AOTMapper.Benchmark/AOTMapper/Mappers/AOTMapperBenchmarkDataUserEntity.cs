using AOTMapper.Benchmark.Data;
using AOTMapper.Core;

namespace AOTMapper.Benchmark.AOTMapper.Mappers;

public static class AOTMapperBenchmarkDataUserEntityExtentions 
{
    [AOTMapperMethod]
    public static User MapToUser(this UserEntity input)
    {
        var output = new User();
        output.FirstName = input.FirstName;
        output.LastName = input.LastName;
        output.Name = $"{input.FirstName} {input.LastName}"; // missing property
        return output;
    }
}
