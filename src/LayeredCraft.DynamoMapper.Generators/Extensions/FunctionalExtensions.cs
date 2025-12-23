namespace System;

internal static class FunctionalExtensions
{
    extension<T, TResult>(T source)
    {
        public TResult Pipe(Func<T, TResult> func) => func(source);
    }

    extension<T>(T source)
    {
        public T Mutate(Action<T> action)
        {
            action(source);
            return source;
        }
    }
}
