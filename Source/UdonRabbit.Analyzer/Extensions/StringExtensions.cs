namespace UdonRabbit.Analyzer.Extensions
{
    public static class StringExtensions
    {
        public static int CountOf(this string obj, string str)
        {
            return (obj.Length - obj.Replace(str, "").Length) / str.Length;
        }
    }
}