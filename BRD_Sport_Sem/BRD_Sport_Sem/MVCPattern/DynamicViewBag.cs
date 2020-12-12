﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ProjectArt.MVCPattern
{
    public class DynamicViewBag : DynamicObject
    {
        private ViewDataDictionary ViewData => _viewDataProvider();

        private readonly Func<ViewDataDictionary> _viewDataProvider;
        
        public DynamicViewBag(Func<ViewDataDictionary> viewDataProvider)
        {
            _viewDataProvider = viewDataProvider;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ViewData.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return ViewData.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!ViewData.ContainsKey(binder.Name))
                return ViewData.TryAdd(binder.Name, value);
            ViewData[binder.Name] = value;
            return true;
        }
    }
}