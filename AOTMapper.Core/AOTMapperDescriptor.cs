using System;

namespace AOTMapper.Core
{
    public interface IAOTMapperDescriptor
    {
        Type Source { get; }

        Type Target { get; }
        
        object Map(IAOTMapper mapper, object source);
    }

    public class AOTMapperDescriptor<TSource, TTarget> : IAOTMapperDescriptor
    {
        public AOTMapperDescriptor(Func<IAOTMapper, TSource, TTarget> mapper)
        {
            Source = typeof(TSource);
            Target = typeof(TTarget);
            MapFunction = mapper;
        }

        public Type Source { get; }

        public Type Target { get; }

        public Func<IAOTMapper, TSource, TTarget> MapFunction { get; }

        public object Map(IAOTMapper mapper, object source)
        {
            return MapFunction(mapper, (TSource)source);
        }
    }
}