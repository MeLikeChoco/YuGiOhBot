namespace YuGiOh.Bot.Extensions
{
    public static class StringArrayExtensions
    {

        public static string ToString(this string[] array, char seperator)
            => array.Join(seperator);

        public static string ToString(this string[] array, string seperator)
            => array.Join(seperator);

    }
}
