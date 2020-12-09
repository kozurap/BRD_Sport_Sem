﻿using System;

namespace ProjectArt.MVCPattern.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateMethodAttribute : Attribute
    {
        
    }
}