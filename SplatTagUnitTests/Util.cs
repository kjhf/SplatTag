using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SplatTagUnitTests
{
  internal static class Util
  {
    // https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout
    public static Task<TResult> RunWithCancellationAsync<TResult>(
      Func<CancellationToken, Task<TResult>> startTask,
      int timeoutMillis, CancellationToken? cancellationToken = null)
      => RunWithCancellationAsync(startTask, TimeSpan.FromMilliseconds(timeoutMillis), cancellationToken);

    public static async Task<TResult> RunWithCancellationAsync<TResult>(
      Func<CancellationToken, Task<TResult>> startTask,
      TimeSpan timeout, CancellationToken? cancellationToken = null)
    {
      Task<TResult> originalTask;
      using var timeoutCancellation = new CancellationTokenSource();
      if (cancellationToken != null)
      {
        using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value, timeoutCancellation.Token);
        originalTask = startTask(combinedCancellation.Token);
      }
      else
      {
        originalTask = startTask(timeoutCancellation.Token);
      }
      var delayTask = Task.Delay(timeout, timeoutCancellation.Token);
      var completedTask = await Task.WhenAny(originalTask, delayTask);
      // Cancel timeout to stop either task:
      // - Either the original task completed, so we need to cancel the delay task.
      // - Or the timeout expired, so we need to cancel the original task.
      // Canceling will not affect a task, that is already completed.
      timeoutCancellation.Cancel();
      if (completedTask == originalTask)
      {
        // original task completed
        return await originalTask;
      }
      else
      {
        // timeout
        throw new TimeoutException();
      }
    }

    public static object? GetPrivateMember<T>(T instance, string memberName)
    {
      return typeof(T)
        .GetField(memberName, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public)
        ?.GetValue(instance);
    }
  }
}