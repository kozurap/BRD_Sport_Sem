using System;

namespace ProjectArt.MVCPattern.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionAttribute : Attribute
    {
        public string Pattern { get; set; }
        public string Method { get; set; } = null;
        public bool IsControllerRelatedPath { get; set; } = true;
    }
}