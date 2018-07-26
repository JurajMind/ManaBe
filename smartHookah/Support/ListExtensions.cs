namespace smartHookah.Support
{
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static List<T> Enque<T>(this List<T> list, T item, int count)
        {
            list.Add(item);
            if (list.Count > count) list.RemoveAt(0);

            return list;
        }
    }
}