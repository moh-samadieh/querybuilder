using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{
    internal class ConditionsCompilerProvider
    {
        private readonly Type compilerType;
        private readonly Dictionary<string, MethodInfo> methodsCache = new Dictionary<string, MethodInfo>();
        private readonly object syncRoot = new object();

        public ConditionsCompilerProvider(Compiler compiler)
        {
            this.compilerType = compiler.GetType();
        }

        public MethodInfo GetMethodInfo(Type clauseType, string methodName)
        {
            // The cache key should take the type and the method name into consideration
            var cacheKey = methodName + "::" + clauseType.FullName;

            lock (syncRoot)
            {
                if (methodsCache.ContainsKey(cacheKey))
                {
                    return methodsCache[cacheKey];
                }

                return methodsCache[cacheKey] = FindMethodInfo(clauseType, methodName);
            }
        }

        private MethodInfo FindMethodInfo(Type clauseType, string methodName)
        {
            MethodInfo methodInfo = compilerType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(x => x.Name == methodName);

            if (methodInfo == null)
            {
                throw new Exception(
                    string.Format("Failed to locate a compiler for '{0}'.", methodName));
            }

            if (clauseType.IsGenericType && methodInfo.IsGenericMethodDefinition)
            {
                methodInfo = methodInfo.MakeGenericMethod(
                    clauseType.GetGenericArguments());
            }

            return methodInfo;
        }
    }
}
