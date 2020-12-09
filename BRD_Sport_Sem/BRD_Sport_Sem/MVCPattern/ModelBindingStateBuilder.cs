﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectArt.MVCPattern
{
    public class ModelBindingStateBuilder
    {
        private Dictionary<string, bool> _constraints;
        private Dictionary<string, ModelBindingState> _states;
        private Dictionary<string, bool> _isSet;

        public ModelBindingStateBuilder()
        {
            _constraints = new Dictionary<string, bool>();
            _states = new Dictionary<string, ModelBindingState>();
            _isSet = new Dictionary<string, bool>();
        }

        public void AddState(string stateName, ModelBindingState state)
            => _states.Add(stateName, state);

        public bool AssertSucceeded(string constraintName)
        {
            _constraints.Add(constraintName, true);
            return true;
        }
        
        public bool AssertFailed(string constraintName)
        {
            _constraints.Add(constraintName, false);
            return false;
        }

        public bool AssertAreEqual(object obj1, object obj2, string constraintName)
        {
            var result = obj1.Equals(obj2);
            _constraints.Add(constraintName, result);
            return result;
        }

        public bool AssertIsTrue(bool check, string constraintName)
        {
            _constraints.Add(constraintName, check);
            return check;
        }

        public bool AssertNotException(Action action, string constraintName)
        {
            try
            {
                action();
                AssertSucceeded(constraintName);
                return true;
            }
            catch
            {
                AssertFailed(constraintName);
                return false;
            }
        }

        public bool AssertThrowException(Action action, Type exceptionType, string constraintName)
        {
            try
            {
                action();
                AssertFailed(constraintName);
                return false;
            }
            catch(Exception e)
            {
                var result = e.GetType() == exceptionType;
                AssertIsTrue(result, constraintName);
                return result;
            }
        }

        public void SetSucceeded(string name)
        {
            _isSet.Add(name, true);
        }

        public void SetFailed(string name)
        {
            _isSet.Add(name, false);
        }

        public void Set(string name, bool isSet)
        {
            _isSet.Add(name, isSet);
        }

        public ModelBindingState Build()
        {
            return new ModelBindingState(_states, _isSet, _constraints);
        }
        
        public bool IsSet(string value)
        {
            if (_isSet.ContainsKey(value))
                return _isSet[value];
            return false;
        }
    }
}