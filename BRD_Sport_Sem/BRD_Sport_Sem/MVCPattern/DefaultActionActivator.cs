﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ProjectArt.MVCPattern.Attributes;
using ProjectArt.MVCPattern.Services;

namespace ProjectArt.MVCPattern
{
    public class DefaultActionActivator : IActionActivator
    {
        private HashSet<Type> _visitedTypes;
        
        public ValueBindingResult BindObject(Type objType, string name, ValueProvider valueProvider, ModelBindingStateBuilder currentBindingStateBuilder)
        {
            var resolvedType = Nullable.GetUnderlyingType(objType) ??
                               objType;
            var suffix = name;
            string value = null;
            while (!string.IsNullOrEmpty(suffix) && value == null)
            {
                value = valueProvider.GetValue(suffix);
                suffix = string.Concat(suffix.SkipWhile(c => c != '.').Skip(1));
            }

            if (resolvedType == typeof(IFormFile))
            {
                if(valueProvider.GetFiles().Count > 0)
                    return new ValueBindingResult(true, valueProvider.GetFiles()[0], resolvedType);
                return new ValueBindingResult(false, null, resolvedType);
            }
            
            if (resolvedType == typeof(List<IFormFile>))
                return new ValueBindingResult(true, valueProvider.GetFiles().ToList(), resolvedType);
            
            
            try
            {
                if (value == null) throw new Exception();
                if (resolvedType == typeof(object))
                {
                    var result = JsonConvert.DeserializeObject(value, resolvedType);
                    return new ValueBindingResult(true, result, resolvedType);
                }
                else
                {
                    var result = Convert.ChangeType(value, resolvedType);
                    return new ValueBindingResult(true, result, resolvedType);
                }
            }
            catch
            {
                try
                {
                    if (value == null) throw new Exception();
                    if (resolvedType == typeof(object))
                    {
                        var result = Convert.ChangeType(value, resolvedType);
                        return new ValueBindingResult(true, result, resolvedType);
                    }
                    else
                    {
                        var result = JsonConvert.DeserializeObject(value, resolvedType);
                        return new ValueBindingResult(true, result, resolvedType);
                    }
                }
                catch
                {
                    if(_visitedTypes.Contains(resolvedType))
                        return new ValueBindingResult(false, null, resolvedType);
                    
                    var ctor = resolvedType.GetConstructor(Type.EmptyTypes);
                    if (ctor == null) return new ValueBindingResult(false, null, resolvedType);
                    var result = Activator.CreateInstance(resolvedType);
                    var innerModelStateBuilder = new ModelBindingStateBuilder();
                    
                    _visitedTypes.Add(resolvedType);
                    foreach (var f in resolvedType.GetFields().Where(f => f.IsPublic))
                    {
                        var obj = BindObject(f.FieldType, $"{name}.{f.Name}".ToLower(), valueProvider, innerModelStateBuilder);
                        if (obj.IsSuccessful)
                        {
                            innerModelStateBuilder.SetSucceeded(f.Name);
                            f.SetValue(result, obj.Result);
                        }
                        else
                        {
                            if (f.GetCustomAttribute<RequiredAttribute>() != null)
                            {
                                _visitedTypes.Remove(resolvedType);
                                return new ValueBindingResult(false, null, resolvedType);
                            }

                            innerModelStateBuilder.SetFailed(f.Name);
                        }
                    }

                    foreach (var p in resolvedType.GetProperties().Where(info => info.GetSetMethod() != null))
                    {
                        var obj = BindObject(p.PropertyType, $"{name}.{p.Name}".ToLower(), valueProvider, innerModelStateBuilder);
                        if (obj.IsSuccessful)
                        {
                            innerModelStateBuilder.SetSucceeded(p.Name);
                            p.SetValue(result, obj.Result);
                        }
                        else
                        {
                            if (p.GetCustomAttribute<RequiredAttribute>() != null)
                            {
                                _visitedTypes.Remove(resolvedType);
                                return new ValueBindingResult(false, null, resolvedType);
                            }

                            innerModelStateBuilder.SetFailed(p.Name);
                        }
                    }
                    
                    _visitedTypes.Remove(resolvedType);

                    var validateMethod = resolvedType.GetMethods().FirstOrDefault(method =>
                        method.GetCustomAttribute<ValidateMethodAttribute>() != null
                        && method.GetParameters().SingleOrDefault() != null
                        && method.GetParameters().SingleOrDefault().ParameterType == typeof(ModelBindingStateBuilder));

                    if (validateMethod != null)
                        validateMethod.Invoke(result, new object?[] {innerModelStateBuilder});

                    var innerModelState = innerModelStateBuilder.Build();
                    
                    if(validateMethod != null)
                        if(validateMethod.GetCustomAttribute<RequiredAttribute>() != null
                           && innerModelState.LocalFailsCount > 0)
                            return new ValueBindingResult(false, null, resolvedType);
                            

                    string lastSuffix = name.Substring(name.LastIndexOf('.') + 1);
                    currentBindingStateBuilder.AddState(lastSuffix,innerModelState);

                    return new ValueBindingResult(true, result, resolvedType);
                }
            }
        }

        public object[] ActivateParameters(HttpContext context, MethodInfo actionMethod, ModelBindingStateBuilder stateBuilder)
        {
            object[] parameters = new object[actionMethod.GetParameters().Length];
            var paramInfos = actionMethod.GetParameters();

            var builder = new ValueProviderBuilder(context);
            builder.Add<RouteValueProviderSource>();
            builder.Add<QueryValueProviderSource>();
            builder.Add<FormValueProviderSource>();
            builder.Add<FormDataJsonValueProviderSource>();
            builder.Add<FormFilesProviderSource>();

            var valueProvider = builder.Build();
            
            foreach (var parameter in paramInfos)
            {
                _visitedTypes = new HashSet<Type>();
                var result = BindObject(parameter.ParameterType, parameter.Name.ToLower(), valueProvider, stateBuilder);
                if (result.IsSuccessful)
                {
                    parameters[parameter.Position] = result.Result;
                    stateBuilder.SetSucceeded(parameter.Name);
                }
                else
                {
                    parameters[parameter.Position] = parameter.DefaultValue?.GetType() == typeof(DBNull)
                            ? null
                            : parameter.DefaultValue;
                    stateBuilder.SetFailed(parameter.Name);
                }
            }

            return parameters;
        }
    }
}