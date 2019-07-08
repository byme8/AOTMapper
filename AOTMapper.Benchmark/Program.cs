using System;
using BenchmarkDotNet.Running;

namespace AOTMapper.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<AOTMapperVSAutoMapper>();
        }
    }
}
