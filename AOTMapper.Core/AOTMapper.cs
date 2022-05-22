using System;
using System.Collections.Generic;
using System.Linq;
using AOTMapper.Core;

namespace AOTMapper
{
    public interface IAOTMapper
    {
        TTarget Map<TTarget>(object source);
        
        IAOTMapperDescriptor[] GetDescriptors();
        
    }

    public class AOTMapper : IAOTMapper
    {
        public AOTMapper(Dictionary<Type, Dictionary<Type, IAOTMapperDescriptor>> mappers)
        {
            Mappers = mappers;
        }

        public Dictionary<Type, Dictionary<Type, IAOTMapperDescriptor>> Mappers { get; set; }

        public TTarget Map<TTarget>(object source)
        {
            if (source is null)
            {
                return default;
            }
            
            var sourceType = source.GetType();
            if (!Mappers.TryGetValue(sourceType, out var descriptors))
            {
                throw new AOTMapperMappingIsMissingException("Mappings is missing for type " + sourceType.FullName);
            }

            var destinationType = typeof(TTarget);
            if (!descriptors.TryGetValue(destinationType, out var descriptor))
            {
                throw new AOTMapperMappingIsMissingException($"Mapping from {sourceType.FullName} to {destinationType.FullName} is missing");
            }

            return (TTarget)descriptor.Map(this, source);
        }

        public IAOTMapperDescriptor[] GetDescriptors()
        {
            return Mappers.SelectMany(o => o.Value.Values).ToArray();
        }
    }
}