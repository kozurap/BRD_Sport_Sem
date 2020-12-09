﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using ProjectArt.MVCPattern.Attributes;
using RazorEngine.Compilation;

namespace ProjectArt.MVCPattern
{
    public class RouteHelper
    {
        class ActionEntity
        {
            public Type ControllerType;
            public ObjectFactory ControllerFactory;
            public MethodInfo ActionMethod;
        }

        private Dictionary<string, Dictionary<string, List<ActionEntity>>> _actionDictionary;
        private IControllerActivator _controllerActivator;
        private IActionActivator _actionActivator;

        public RouteHelper(IControllerActivator controllerActivator, IActionActivator actionActivator)
        {
            _controllerActivator = controllerActivator;
            _actionActivator = actionActivator;
            _actionDictionary = new Dictionary<string, Dictionary<string, List<ActionEntity>>>();
        }

        public void Initialize(RouteBuilder routeBuilder)
        {
            routeBuilder.DefaultHandler = new RouteHandler(Handle);
            foreach (var controller in Assembly.GetExecutingAssembly().GetTypes()
                .Where(type =>
                    type.GetCustomAttribute<ControllerAttribute>() != null && type.IsSubclassOf(typeof(Controller))))
            {
                var controllerAttribute = controller.GetCustomAttribute<ControllerAttribute>();
                var controllerPath = !string.IsNullOrEmpty(controllerAttribute.ControllerName) ?
                    $"{controllerAttribute.ControllerName}{(controllerAttribute.ControllerName[^1] == '/' ? "" : "/")}" : "";
                
                if (!_actionDictionary.ContainsKey(controller.Name))
                    _actionDictionary.Add(controller.Name,
                        new Dictionary<string, List<ActionEntity>>());
                
                foreach (var action in controller.GetMethods())
                {
                    var actionAttributes = action.GetCustomAttributes<ActionAttribute>().ToArray();
                    for (int i = 0; i < actionAttributes.Length; i++)
                    {
                        try
                        {
                            if (!_actionDictionary[controller.Name].ContainsKey(action.Name))
                                _actionDictionary[controller.Name].Add(action.Name, new List<ActionEntity>());
                            _actionDictionary[controller.Name][action.Name].Add(
                                new ActionEntity()
                                {
                                    ControllerType = controller,
                                    ActionMethod = action,
                                    ControllerFactory = ActivatorUtilities.CreateFactory(controller, new Type[0])
                                });
                            object constraints = null;
                            if (actionAttributes[i].Method != MethodType.ALL)
                            {
                                var methods = new List<string>();
                                if((actionAttributes[i].Method & MethodType.GET) == MethodType.GET)
                                    methods.Add("GET");
                                if((actionAttributes[i].Method & MethodType.POST) == MethodType.POST)
                                    methods.Add("POST");
                                if((actionAttributes[i].Method & MethodType.DELETE) == MethodType.DELETE)
                                    methods.Add("DELETE");
                                if((actionAttributes[i].Method & MethodType.PUT) == MethodType.PUT)
                                    methods.Add("PUT");
                                constraints = new
                                    {httpMethod = new HttpMethodRouteConstraint(methods.ToArray())};
                            }
                            else constraints = new { };

                            var isControllerRelatedPath = actionAttributes[i].IsControllerRelatedPath;
                            if (actionAttributes[i].Pattern.StartsWith("~/"))
                            {
                                isControllerRelatedPath = false;
                                actionAttributes[i].Pattern = actionAttributes[i].Pattern.Substring(2);
                            }
                            
                            if(actionAttributes[i].Pattern.StartsWith('/'))
                                actionAttributes[i].Pattern = actionAttributes[i].Pattern.Substring(1);

                            var index = _actionDictionary[controller.Name][action.Name].Count - 1;
                            routeBuilder.MapRoute($"{controller.Name}.{action.Name}.{index}"
                                , (isControllerRelatedPath
                                    ? controllerPath + actionAttributes[i].Pattern
                                    : actionAttributes[i].Pattern),
                                new { }, constraints);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
        }

        private async Task Handle(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var route = (Route) routeData.Routers[^2];
            var splitTemp = route.Name.Split('.');
            string controllerName = splitTemp[0], actionName = splitTemp[1];
            int index = int.Parse(splitTemp[2]);

            var actionEntity = _actionDictionary[controllerName][actionName][index];

            var modelStateBuilder = new ModelBindingStateBuilder();
            var parameters = _actionActivator.ActivateParameters(context, actionEntity.ActionMethod, modelStateBuilder);
            if (parameters == null)
            {
                context.Response.StatusCode = 400;
                return;
            }
            
            var controllerObject =
                _controllerActivator.Activate(context, route, actionEntity.ControllerFactory, modelStateBuilder.Build());
            var controller = (Controller) controllerObject;

            controller.RouteValues["controller"] = controllerName.EndsWith("Controller")
                ? controllerName.Substring(0, controllerName.Length - "Controller".Length) : controllerName;
            controller.RouteValues["action"] = actionName;

            IActionResult actionResult = null;
            var result = actionEntity.ActionMethod.Invoke(controllerObject, parameters);
            if (result is Task<IActionResult> task)
                actionResult = await task;
            else if (result is IActionResult iActionResult)
                actionResult = iActionResult;

            await actionResult.ExecuteResult(controller);
        }
    }
}