using System;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class TypeExtensions
    {
        // https://stackoverflow.com/questions/6386202/get-type-name-without-any-generics-info
        public static string GetNameWithoutGenericArity(this Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            var name = t.Name;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }
    }
}