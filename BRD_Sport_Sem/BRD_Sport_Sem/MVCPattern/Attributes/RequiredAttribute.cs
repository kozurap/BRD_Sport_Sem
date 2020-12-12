﻿using System;

namespace ProjectArt.MVCPattern.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class RequiredAttribute : Attribute
    {
        
    }
}