using System;
using System.Collections.Generic;
using System.Linq;

namespace AOTMapper.Core
{
    public class AOTMapperBuilder
    {
        public AOTMapperBuilder()
        {
            Descriptors = new List<IAOTMapperDescriptor>();
        }

        public List<IAOTMapperDescriptor> Descriptors { get; set; }

        public static AOTMapperBuilder FromMappers(params IAOTMapper[] mappers)
        {
            var descriptors = mappers
                .SelectMany(o => o.GetDescriptors())
                .ToArray();

            var builder = new AOTMapperBuilder();
            builder.Descriptors.AddRange(descriptors);

            return builder;
        }

        public AOTMapperBuilder AddMapper<TSource, TTarget>(Func<IAOTMapper, TSource, TTarget> mapper)
        {
            Descriptors.Add(new AOTMapperDescriptor<TSource, TTarget>(mapper));
            return this;
        }

        public IAOTMapper Build()
        {
            var groupedMappers = Descriptors
                .GroupBy(o => o.Source)
                .Select(groupedBySource =>
                {
                    var sourceMappers = groupedBySource
                        .ToDictionary(o => o.Target);

                    return (Key: groupedBySource.Key, Value: sourceMappers);
                })
                .ToDictionary(o => o.Key, o => o.Value);

            return new AOTMapper(groupedMappers);
        }
    }
}