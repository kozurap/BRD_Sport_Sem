﻿using System;

namespace ProjectArt.MVCPattern
{
    public class ValueBindingResult
    {
        public ValueBindingResult(bool isSuccessful, object result, Type resultType)
        {
            IsSuccessful = isSuccessful;
            Result = result;
            ResultType = resultType;
        }

        public bool IsSuccessful { get; }
        public object Result { get; }
        public Type ResultType { get; }
    }
}