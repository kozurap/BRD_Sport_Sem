﻿using System;

namespace ProjectArt.MVCPattern.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerAttribute : Attribute
    {
        public string ControllerName { get; }

        public ControllerAttribute(string controllerName)
        {
            ControllerName = controllerName;
        }
    }
}