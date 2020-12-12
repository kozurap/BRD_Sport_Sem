﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectArt.MVCPattern.Services
{
    public interface IActionResultProvider<TResult> where TResult : IActionResult
    {
        TResult GetResult(params object[] args);
    }
    
    public class ActionResultProvider<TResult> : IActionResultProvider<TResult> where TResult : IActionResult
    {
        private IServiceProvider _serviceProvider;
        private ObjectFactory _actionFactory;

        public ActionResultProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public TResult GetResult(params object[] args)
        {
            return (TResult)ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, args.Where(obj => obj != null).ToArray());
        }
    }
}