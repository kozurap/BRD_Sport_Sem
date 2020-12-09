﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectArt.MVCPattern
{
    public class ModelBindingState : IEnumerable<KeyValuePair<string, ModelBindingState>>
    {
        private readonly Dictionary<string, bool> _constraints;
        private readonly Dictionary<string, ModelBindingState> _states;
        private readonly Dictionary<string, bool> _isSet;

        public ModelBindingState this[string state]
        {
            get
            {
                state = state.ToLower();
                if (_states.ContainsKey(state))
                    return _states[state];
                return null;
            }
        }

        public bool CheckConstraint(string constraintName)
        {
            return CheckLocalConstraint(constraintName)
                   || _states.Any(state 
                => state.Value.CheckConstraint(constraintName));
        }

        public bool CheckLocalConstraint(string constraintName)
        {
            if (_constraints.ContainsKey(constraintName))
                return _constraints[constraintName];
            return false;
        }

        private int? _localFailsCount = null;

        public int LocalFailsCount
        {
            get
            {
                if(_localFailsCount == null)
                    _localFailsCount = _constraints
                        .Count(constraint => constraint.Value == false);
                return _localFailsCount.Value;
            }
        }

        private int? _failsCount = null;
        public int FailsCount
        {
            get
            {
                if (_failsCount == null)
                {
                    _failsCount = LocalFailsCount;

                    foreach (var failsCount in _states.Values.Select(state => state.FailsCount))
                        _failsCount += failsCount;
                }

                return _failsCount.Value;
            }
        }

        private bool? _isAllSet;
        public bool IsAllSet
        {
            get
            {
                if (_isAllSet == null)
                {
                    _isAllSet = true;
                    foreach (var isSet in _isSet)
                        if (!isSet.Value)
                            _isAllSet = false;
                }

                return _isAllSet.Value;
            }
        }

        private bool? _isAllSetCascade;
        public bool IsAllSetCascade
        {
            get
            {
                if (_isAllSetCascade == null)
                {
                    _isAllSetCascade = true;
                    if (!IsAllSet) _isAllSetCascade = false;
                    foreach (var isAllSetCascade in _states.Values.Select(state => state.IsAllSetCascade))
                        if (!isAllSetCascade)
                            _isAllSetCascade = false;
                }

                return _isAllSetCascade.Value;
            }
        }

        public IEnumerable<KeyValuePair<string, bool>> Constraints
        {
            get
            {
                return _constraints.Concat(_states
                    .SelectMany(state
                        => state.Value.Constraints));
            }
        }

        public IEnumerable<KeyValuePair<string, bool>> LocalConstraints => _constraints;

        public IEnumerable<string> FailedConstraints
        {
            get
            {
                return Constraints.Where(constraint => !constraint.Value)
                    .Select(constraint => constraint.Key);
            }
        }

        public IEnumerable<string> LocalFailedConstraints
        {
            get => LocalConstraints
                .Where(constraint => constraint.Value)
                .Select(constraint => constraint.Key);
        }

        public ModelBindingState(Dictionary<string, ModelBindingState> states
            , Dictionary<string, bool> isSet, Dictionary<string, bool> constraints)
        {
            _states = states;
            _isSet = isSet;
            _constraints = constraints;
        }

        public bool IsSet(string name)
        {
            if (_isSet.ContainsKey(name))
                return _isSet[name];
            return false;
        }

        public bool IsSetCascade(string name)
        {
            if (IsSet(name))
                return true;
            foreach(var state in _states.Values)
                if (state.IsSetCascade(name))
                    return true;
            return false;
        }

        public IEnumerator<KeyValuePair<string, ModelBindingState>> GetEnumerator() 
            => _states.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }
}