namespace System.Collections.Generic;

internal static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public void ForEach(Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }

    extension<T>(IEnumerable<T?> valueProviders)
        where T : struct
    {
        public IEnumerable<T> WhereNotNull() =>
            valueProviders.Where(static v => v is not null).Select(static v => v!.Value);
    }
}
