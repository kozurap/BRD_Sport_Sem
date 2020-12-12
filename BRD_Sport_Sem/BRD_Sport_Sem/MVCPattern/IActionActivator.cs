﻿using System.Reflection;
using Microsoft.AspNetCore.Http;
using ProjectArt.MVCPattern.Attributes;

namespace ProjectArt.MVCPattern
{
    public interface IActionActivator
    {
        object[] ActivateParameters(HttpContext context, MethodInfo actionMethod, ModelBindingStateBuilder modelStateBuilder);
    }
}