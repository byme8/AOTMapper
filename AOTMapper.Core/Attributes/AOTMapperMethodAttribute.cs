using System;

namespace AOTMapper.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AOTMapperMethodAttribute : Attribute
    {
        public AOTMapperMethodAttribute(bool disableMissingPropertiesDetection = false)
        {
            DisableMissingPropertiesDetection = disableMissingPropertiesDetection;
        }

        public bool DisableMissingPropertiesDetection { get; set; }
    }
}