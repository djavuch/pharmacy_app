using PharmacyApp.Application.Interfaces.Abstractions;

namespace PharmacyApp.IntegrationTests.Support;

internal sealed class NoOpBackgroundTaskQueue : IBackgroundTaskQueue
{
    public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<Func<CancellationToken, ValueTask>>(_ => ValueTask.CompletedTask);
    }
}
