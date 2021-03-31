using System.Collections.Generic;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddIfValid<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (key != null)
                dict.Add(key, value);
        }
    }
}