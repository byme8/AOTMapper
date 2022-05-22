using System;
using System.Runtime.Serialization;

namespace AOTMapper.Core
{
    [Serializable]
    public class AOTMapperMappingIsMissingException : Exception
    {
        public AOTMapperMappingIsMissingException()
        {
        }

        public AOTMapperMappingIsMissingException(string message)
            : base(message)
        {
        }

        public AOTMapperMappingIsMissingException(string message, Exception inner) 
            : base(message, inner)
        {
        }

        protected AOTMapperMappingIsMissingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}