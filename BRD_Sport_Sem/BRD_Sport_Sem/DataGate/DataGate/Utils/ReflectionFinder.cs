using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataGate.Utils
{
    public static class ReflectionFinder
    {
        public static MethodInfo Substring
            = typeof(string).GetMethod("Substring"
                , new Type[] {typeof(int), typeof(int)});
        
        private static Dictionary<Type, Dictionary<string, MemberInfo[]>> _cachedMembers
            = new Dictionary<Type, Dictionary<string, MemberInfo[]>>();
        
        private static Dictionary<Type, Dictionary<string, MemberInfo>> _cachedMembersSingle
            = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        
        private static Dictionary<Type, Dictionary<string, MethodInfo>> _cachedMethods
            = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        
        public static MemberInfo[] GetMemberInfos(Type type, string name)
        {
            if(!_cachedMembers.ContainsKey(type))
                _cachedMembers.Add(type, new Dictionary<string, MemberInfo[]>());

            if (!_cachedMembers[type].ContainsKey(name))
                _cachedMembers[type].Add(name, type.GetMember(name));
            
            return _cachedMembers[type][name];
        }
        
        public static MemberInfo GetMemberInfoSingle(Type type, string name)
        {
            var s = ";";
            if(!_cachedMembersSingle.ContainsKey(type))
                _cachedMembersSingle.Add(type, new Dictionary<string, MemberInfo>());

            if (!_cachedMembersSingle[type].ContainsKey(name))
                _cachedMembersSingle[type].Add(name, type.GetMember(name)[0]);
            
            return _cachedMembersSingle[type][name];
        }

        public static MemberInfo[] GetMemberInfos<T>(string name)
            => GetMemberInfos(typeof(T), name);

        public static MemberInfo GetMemberInfoSingle<T>(string name)
            => GetMemberInfoSingle(typeof(T), name);

    }
}