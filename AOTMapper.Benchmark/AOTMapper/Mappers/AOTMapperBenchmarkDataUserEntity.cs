
public static class AOTMapperBenchmarkDataUserEntityExtentions 
{
    public static AOTMapper.Benchmark.Data.User MapToUser(this AOTMapper.Benchmark.Data.UserEntity input)
    {
        var output = new AOTMapper.Benchmark.Data.User();
        output.FirstName = input.FirstName;
        output.LastName = input.LastName;
        output.Name = $"{input.FirstName} {input.LastName}"; // missing property
        return output;
    }
}
