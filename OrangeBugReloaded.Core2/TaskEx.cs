using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Maps calls to the TaskEx-class of the Async/Tasks-backport to the Task-class.
    /// </summary>
    public static class TaskEx
    {
        public static Task Delay(TimeSpan dueTime) => Task.Delay(dueTime);
        public static Task Delay(int dueTime) => Task.Delay(dueTime);
        public static Task Delay(int dueTime, CancellationToken cancellationToken) => Task.Delay(dueTime, cancellationToken);
        public static Task Delay(TimeSpan dueTime, CancellationToken cancellationToken) => Task.Delay(dueTime, cancellationToken);
        public static Task<TResult> FromResult<TResult>(TResult result) => Task.FromResult(result);
        public static Task Run(Func<Task> function) => Task.Run(function);
        public static Task Run(Action action) => Task.Run(action);
        public static Task Run(Func<Task> function, CancellationToken cancellationToken) => Task.Run(function, cancellationToken);
        public static Task Run(Action action, CancellationToken cancellationToken) => Task.Run(action, cancellationToken);
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function) => Task.Run(function);
        public static Task<TResult> Run<TResult>(Func<TResult> function) => Task.Run(function);
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken) => Task.Run(function, cancellationToken);
        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken) => Task.Run(function, cancellationToken);
        public static Task WhenAll(IEnumerable<Task> tasks) => Task.WhenAll(tasks);
        public static Task WhenAll(params Task[] tasks) => Task.WhenAll(tasks);
        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) => Task.WhenAll(tasks);
        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks) => Task.WhenAll(tasks);
        public static Task<Task> WhenAny(IEnumerable<Task> tasks) => Task.WhenAny(tasks);
        public static Task<Task> WhenAny(params Task[] tasks) => Task.WhenAny(tasks);
        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks) => Task.WhenAny(tasks);
        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks) => Task.WhenAny(tasks);
        public static YieldAwaitable Yield() => Task.Yield();
    }
}
