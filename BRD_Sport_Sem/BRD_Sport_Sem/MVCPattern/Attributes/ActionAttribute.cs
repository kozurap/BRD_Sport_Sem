﻿using System;

namespace ProjectArt.MVCPattern.Attributes
{
    [Flags]
    public enum MethodType
    {
        ALL = 0,
        GET = 1,
        POST = 2,
        DELETE = 4,
        PUT = 8
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionAttribute : Attribute
    {
        public string Pattern { get; set; }
        public MethodType Method { get; set; } = MethodType.ALL;
        public bool IsControllerRelatedPath { get; set; } = true;

        public ActionAttribute(string pattern)
        {
            Pattern = pattern;
        }

        public ActionAttribute()
        {
            
        }
    }
}